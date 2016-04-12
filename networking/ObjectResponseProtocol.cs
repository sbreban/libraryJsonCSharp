using System;

namespace networking
{
    public enum ResponseType
    {
        LOGIN_SUCCESSFULLY, LOGOUT_SUCCESSFULLY, GET_AVAILABLE_BOOKS, GET_USER_BOOKS, SEARCH_BOOKS, BORROW_BOOK, RETURN_BOOK, OK, ERROR
    }

    [Serializable]
	public class Response
    {
        private ResponseType type;

        private Object data;

        public Response(ResponseType type, object data)
        {
            this.type = type;
            this.data = data;
        }

        public ResponseType Type
        {
            get { return type; }
            set { type = value; }
        }

        public object Data
        {
            get { return data; }
            set { data = value; }
        }
    }
}