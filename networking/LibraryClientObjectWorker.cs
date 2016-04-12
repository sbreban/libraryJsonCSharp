using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using model;
using networking.dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using services;

namespace networking
{
    public class LibraryClientWorker : ILibraryClient
    {
        private ILibraryServer server;
        private TcpClient connection;

        private NetworkStream stream;

        private StreamWriter writer;
        private StreamReader reader;

        private volatile bool connected;

        public LibraryClientWorker(ILibraryServer server, TcpClient connection)
        {
            this.server = server;
            this.connection = connection;
            try
            {
                stream = connection.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                connected = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public virtual void run()
        {
            while (connected)
            {
                try
                {
                    String requestJson = reader.ReadLine();
                    Console.WriteLine("Request received json " + requestJson);
                    Request request = JsonConvert.DeserializeObject<Request>(requestJson, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Converters = new List<JsonConverter>() { new RequestConverter() }
                    });

                    Response response = handleRequest(request);

                    if (response != null)
                    {
                        sendResponse((Response)response);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }

                try
                {
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
            try
            {
                reader.Close();
                writer.Close();
                stream.Close();
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error " + e);
            }
        }

        public void bookUpdated(int bookId, int newQuantity)
        {
            BookQuantityDTO bookQuantityDto = new BookQuantityDTO(bookId, newQuantity);
            sendResponse(new Response(ResponseType.BORROW_BOOK, bookQuantityDto));
        }

        public void bookReturned(int bookId, string author, string title)
        {
            BookDTO bookDto = new BookDTO(bookId, author, title);
            sendResponse(new Response(ResponseType.RETURN_BOOK, bookDto));
        }

        private Response handleRequest(Request request)
        {
            Response response = null;

            if (request.Type == RequestType.LOGIN)
            {
                Console.WriteLine("Login request ...");
                UserDTO userDto = (UserDTO)request.Data;
                try
                {
                    User user = null;
                    lock (server)
                    {
                        user = server.login(userDto.UserName, userDto.Password, this);
                    }
                    return new Response(ResponseType.LOGIN_SUCCESSFULLY, user);
                }
                catch (LibraryException e)
                {
                    connected = false;
                    return new Response(ResponseType.ERROR, e.Message);
                }
            }

            if (request.Type == RequestType.LOGOUT)
            {
                Console.WriteLine("Logout request");
                int userId = (int)request.Data;
                try
                {
                    lock (server)
                    {

                        server.logout(userId, this);
                    }
                    connected = false;
                    return new Response(ResponseType.LOGOUT_SUCCESSFULLY, null);
                }
                catch (LibraryException e)
                {
                    return new Response(ResponseType.ERROR, e.Message);
                }
            }

            if (request.Type == RequestType.GET_AVAILABLE_BOOKS)
            {
                Console.WriteLine("SendMessageRequest ...");
                try
                {
                    List<Book> availableBooks = null;
                    lock (server)
                    {
                        availableBooks = server.getAvailableBooks();
                    }
                    return new Response(ResponseType.GET_AVAILABLE_BOOKS, availableBooks);
                }
                catch (LibraryException e)
                {
                    return new Response(ResponseType.ERROR, e.Message);
                }
            }

            if (request.Type == RequestType.GET_USER_BOOKS)
            {
                Console.WriteLine("GetLoggedFriends Request ...");
                int userId = (int)request.Data;
                try
                {
                    List<Book> userBooks = null;
                    lock (server)
                    {
                        userBooks = server.getUserBooks(userId);
                    }
                    return new Response(ResponseType.GET_USER_BOOKS, userBooks);
                }
                catch (LibraryException e)
                {
                    return new Response(ResponseType.ERROR, e.Message);
                }
            }

            if (request.Type == RequestType.SEARCH_BOOKS)
            {
                Console.WriteLine("GetLoggedFriends Request ...");
                String searchKey = (String)request.Data;
                try
                {
                    List<Book> foundBooks = null;
                    lock (server)
                    {
                        foundBooks = server.searchBooks(searchKey);
                    }
                    return new Response(ResponseType.SEARCH_BOOKS, foundBooks);
                }
                catch (LibraryException e)
                {
                    return new Response(ResponseType.ERROR, e.Message);
                }
            }

            if (request.Type == RequestType.BORROW_BOOK)
            {
                Console.WriteLine("GetLoggedFriends Request ...");
                UserBookDTO userBookDto = (UserBookDTO)request.Data;
                try
                {
                    lock (server)
                    {
                        server.borrowBook(userBookDto.UserId, userBookDto.BookId);
                    }
                    return new Response(ResponseType.OK, null);
                }
                catch (LibraryException e)
                {
                    return new Response(ResponseType.ERROR, e.Message);
                }
            }

            if (request.Type == RequestType.RETURN_BOOK)
            {
                Console.WriteLine("GetLoggedFriends Request ...");
                UserBookDTO userBookDto = (UserBookDTO)request.Data;
                try
                {
                    lock (server)
                    {
                        server.returnBook(userBookDto.UserId, userBookDto.BookId);
                    }
                    return new Response(ResponseType.OK, null);
                }
                catch (LibraryException e)
                {
                    return new Response(ResponseType.ERROR, e.Message);
                }
            }

            return response;
        }

        private void sendResponse(Response response)
        {
            string json = JsonConvert.SerializeObject(response, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter>() { new StringEnumConverter() }
            });
            Console.WriteLine("Sending response json" + json);
            writer.WriteLine(json);
            writer.Flush();
        }
    }

}