using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using model;
using networking.dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using services;

namespace networking
{
    public class LibraryServerObjectProxy : ILibraryServer
    {
        private string host;
        private int port;

        private ILibraryClient client;

        private NetworkStream stream;

        private StreamReader reader;
        private StreamWriter writer;

        private TcpClient connection;

        private Queue<Response> responses;
        private volatile bool finished;
        private EventWaitHandle _waitHandle;
        public LibraryServerObjectProxy(string host, int port)
        {
            this.host = host;
            this.port = port;
            responses = new Queue<Response>();
        }

        public User login(string userName, string password, ILibraryClient client)
        {
            initializeConnection();
            User user = null;
            UserDTO userDto = new UserDTO(userName, password);
            sendRequest(new Request(RequestType.LOGIN, userDto));
            Response response = readResponse();
            if (response.Type == ResponseType.ERROR)
            {
                string err = (string)response.Data;
                closeConnection();
                throw new LibraryException(err);
            }
            if (response.Type == ResponseType.LOGIN_SUCCESSFULLY)
            {
                user = (User)response.Data;
                this.client = client;
            }
            return user;
        }

        public void logout(int userId, ILibraryClient client)
        {
            sendRequest(new Request(RequestType.LOGOUT, userId));
            Response response = readResponse();
            closeConnection();
            if (response.Type == ResponseType.ERROR)
            {
                string err = (string)response.Data;
                throw new LibraryException(err);
            }
        }

        public List<Book> getAvailableBooks()
        {
            sendRequest(new Request(RequestType.GET_AVAILABLE_BOOKS, null));
            Response response = readResponse();
            List<Book> availableBooks = null;
            if (response.Type == ResponseType.ERROR)
            {
                string err = (string)response.Data;
                throw new LibraryException(err);
            }
            if (response.Type == ResponseType.GET_AVAILABLE_BOOKS)
            {
                availableBooks = (List<Book>)response.Data;
            }
            return availableBooks;
        }

        public List<Book> getUserBooks(int userId)
        {
            sendRequest(new Request(RequestType.GET_USER_BOOKS, userId));
            Response response = readResponse();
            List<Book> userBooks = null;
            if (response.Type == ResponseType.ERROR)
            {
                string err = (string)response.Data;
                throw new LibraryException(err);
            }
            if (response.Type == ResponseType.GET_USER_BOOKS)
            {
                userBooks = (List<Book>)response.Data;
            }
            return userBooks;
        }

        public List<Book> searchBooks(string key)
        {
            sendRequest(new Request(RequestType.SEARCH_BOOKS, key));
            Response response = readResponse();
            List<Book> foundBooks = null;
            if (response.Type == ResponseType.ERROR)
            {
                string err = (string)response.Data;
                throw new LibraryException(err);
            }
            if (response.Type == ResponseType.SEARCH_BOOKS)
            {
                foundBooks = (List<Book>)response.Data;
            }
            return foundBooks;
        }

        public void borrowBook(int userId, int bookId)
        {
            UserBookDTO userBookDto = new UserBookDTO(userId, bookId);
            sendRequest(new Request(RequestType.BORROW_BOOK, userBookDto));
            Response response = readResponse();
            if (response.Type == ResponseType.ERROR)
            {
                string err = (string)response.Data;
                throw new LibraryException(err);
            }
        }

        public void returnBook(int userId, int bookId)
        {
            UserBookDTO userBookDto = new UserBookDTO(userId, bookId);
            sendRequest(new Request(RequestType.RETURN_BOOK, userBookDto));
            Response response = readResponse();
            if (response.Type == ResponseType.ERROR)
            {
                string err = (string)response.Data;
                throw new LibraryException(err);
            }
        }

        private void closeConnection()
        {
            finished = true;
            try
            {
                reader.Close();
                writer.Close();
                stream.Close();
                connection.Close();
                _waitHandle.Close();
                client = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        private void sendRequest(Request request)
        {
            try
            {
                string json = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = new List<JsonConverter>() { new StringEnumConverter() }
                });
                Console.WriteLine("Sending request json" + json);
                writer.WriteLine(json);
                writer.Flush();
            }
            catch (Exception e)
            {
                throw new LibraryException("Error sending object " + e);
            }

        }

        private Response readResponse()
        {
            Response response = null;
            try
            {
                _waitHandle.WaitOne();
                lock (responses)
                {
                    //Monitor.Wait(responses); 
                    response = responses.Dequeue();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            return response;
        }

        private void initializeConnection()
        {
            try
            {
                connection = new TcpClient(host, port);
                stream = connection.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                finished = false;
                _waitHandle = new AutoResetEvent(false);
                startReader();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        private void startReader()
        {
            Thread tw = new Thread(run);
            tw.Start();
        }

        private void handleUpdate(Response response)
        {
            if (response.Type == ResponseType.BORROW_BOOK)
            {
                BookQuantityDTO bookQuantityDto = (BookQuantityDTO)response.Data;
                client.bookUpdated(bookQuantityDto.BookId, bookQuantityDto.NewQuantity);
            }
            if (response.Type == ResponseType.RETURN_BOOK)
            {
                BookDTO bookDto = (BookDTO)response.Data;
                client.bookReturned(bookDto.Id, bookDto.Author, bookDto.Title);
            }
        }

        public virtual void run()
        {
            while (!finished)
            {
                try
                {
                    String responseJson = reader.ReadLine();
                    Console.WriteLine("Response received json " + responseJson);
                    Response response = JsonConvert.DeserializeObject<Response>(responseJson, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Converters = new List<JsonConverter>() { new ResponseConverter()}
                    });
                    if (response.Type == ResponseType.BORROW_BOOK || response.Type == ResponseType.RETURN_BOOK)
                    {
                        handleUpdate(response);
                    }
                    else
                    {
                        lock (responses)
                        {
                            responses.Enqueue(response);
                        }
                        _waitHandle.Set();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Reading error " + e);
                }

            }
        }
    }

}