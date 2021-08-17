using System;
using System.Collections.Generic;
using System.Text;

namespace todojuanes.Common.Responses
{
   public  class Response
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public object Result { get; set; }
    }
}
