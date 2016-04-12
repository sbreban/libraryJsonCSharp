using System;

namespace networking
{
    public enum RequestType
    {
        LOGIN, LOGOUT, GET_AVAILABLE_BOOKS, GET_USER_BOOKS, SEARCH_BOOKS, BORROW_BOOK, RETURN_BOOK
    }

    [Serializable]
	public class Request
	{
	    private RequestType type;
	    private Object data;

	    public Request(RequestType type, object data)
	    {
	        this.type = type;
	        this.data = data;
	    }

        public RequestType Type
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