using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Simple_Notes_Tests.DTOs
{  
        public class ApiResponseDto
        {
            [JsonPropertyName("msg")]
            public string Msg { get; set; }
        }

}
