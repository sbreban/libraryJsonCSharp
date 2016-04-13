using System;
using networking.dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace networking.json
{
    class RequestConverter : JsonCreationConverter<Request>
    {
        protected override Request Create(Type objectType, JObject jObject)
        {
            string type = (string)jObject.Property("type");

            if (type.Equals(RequestType.LOGIN.ToString()))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                UserDTO userDto = jsonSerializer.Deserialize<UserDTO>(jObject.Property("data").Value.CreateReader());
                return new Request(RequestType.LOGIN, userDto);
            }
            if (type.Equals(RequestType.LOGOUT.ToString()))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                int userId = jsonSerializer.Deserialize<int>(jObject.Property("data").Value.CreateReader());
                return new Request(RequestType.LOGOUT, userId);
            }
            if (type.Equals(RequestType.GET_AVAILABLE_BOOKS.ToString()))
            {
                return new Request(RequestType.GET_AVAILABLE_BOOKS, null);
            }
            if (type.Equals(RequestType.GET_USER_BOOKS.ToString()))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                int userId = jsonSerializer.Deserialize<int>(jObject.Property("data").Value.CreateReader());
                return new Request(RequestType.GET_USER_BOOKS, userId);
            }
            if (type.Equals(RequestType.SEARCH_BOOKS.ToString()))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                string searchKey = jsonSerializer.Deserialize<string>(jObject.Property("data").Value.CreateReader());
                return new Request(RequestType.SEARCH_BOOKS, searchKey);
            }
            if (type.Equals(RequestType.BORROW_BOOK.ToString()))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                UserBookDTO userBookDto = jsonSerializer.Deserialize<UserBookDTO>(jObject.Property("data").Value.CreateReader());
                return new Request(RequestType.BORROW_BOOK, userBookDto);
            }
            if (type.Equals(RequestType.RETURN_BOOK.ToString()))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                UserBookDTO userBookDto = jsonSerializer.Deserialize<UserBookDTO>(jObject.Property("data").Value.CreateReader());
                return new Request(RequestType.RETURN_BOOK, userBookDto);
            }

            throw new ApplicationException(String.Format("The given vehicle type {0} is not supported!", type));
        }
    }
}
