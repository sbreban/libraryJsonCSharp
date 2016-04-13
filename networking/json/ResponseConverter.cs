using System;
using System.Collections.Generic;
using model;
using networking.dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace networking.json
{
    class ResponseConverter : JsonCreationConverter<Response>
    {
        protected override Response Create(Type objectType, JObject jObject)
        {
            string type = (string)jObject.Property("type");
            if (type.Equals(ResponseType.LOGIN_SUCCESSFULLY.ToString()))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                User user = jsonSerializer.Deserialize<User>(jObject.Property("data").Value.CreateReader());
                return new Response(ResponseType.LOGIN_SUCCESSFULLY, user);
            }
            if (type.Equals(ResponseType.LOGOUT_SUCCESSFULLY.ToString()))
            {
                return new Response(ResponseType.LOGOUT_SUCCESSFULLY, null);
            }
            if (type.Equals(ResponseType.GET_AVAILABLE_BOOKS.ToString()))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                List<Book> availableBooks = jsonSerializer.Deserialize<
                                            List<Book>>(jObject.Property("data").Value.CreateReader());
                return new Response(ResponseType.GET_AVAILABLE_BOOKS, availableBooks);
            }
            if (type.Equals(ResponseType.GET_USER_BOOKS.ToString()))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                List<Book> userBooks = jsonSerializer.Deserialize<
                                            List<Book>>(jObject.Property("data").Value.CreateReader());
                return new Response(ResponseType.GET_USER_BOOKS, userBooks);
            }
            if (type.Equals(ResponseType.SEARCH_BOOKS.ToString()))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                List<Book> foundBooks = jsonSerializer.Deserialize<
                                            List<Book>>(jObject.Property("data").Value.CreateReader());
                return new Response(ResponseType.SEARCH_BOOKS, foundBooks);
            }
            if (type.Equals(ResponseType.OK.ToString()))
            {
                return new Response(ResponseType.OK, null);
            }
            if (type.Equals(ResponseType.ERROR.ToString()))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                string errorMessage = jsonSerializer.Deserialize<string>(jObject.Property("data").Value.CreateReader());
                return new Response(ResponseType.ERROR, errorMessage);
            }
            if (type.Equals(ResponseType.BORROW_BOOK.ToString()))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                BookBorrowedDTO bookBorrowedDto = jsonSerializer.Deserialize<BookBorrowedDTO>(jObject.Property("data").Value.CreateReader());
                return new Response(ResponseType.BORROW_BOOK, bookBorrowedDto);
            }
            if (type.Equals(ResponseType.RETURN_BOOK.ToString()))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                BookReturnedDTO bookReturnedDto = jsonSerializer.Deserialize<BookReturnedDTO>(jObject.Property("data").Value.CreateReader());
                return new Response(ResponseType.RETURN_BOOK, bookReturnedDto);
            }
            throw new ApplicationException(String.Format("The given vehicle type {0} is not supported!", type));
        }
    }
}
