using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RemoteFunctionCall
{
    public class RfcResultDto
    {
        public bool IsSuccess { get; set; }
        public bool IsTimeout { get; set; }
        public string Message { get; set; } = "";
        public Dictionary<string, string> Parameters { get; set; } = null!;

        public RfcResultDto()
        {
        }

        public RfcResultDto(bool isSuccess, bool isTimeout, string message,
            params KeyValuePair<string, string>[] parameters)
        {
            IsSuccess = isSuccess;
            IsTimeout = isTimeout;
            Message = message;
            Parameters = new Dictionary<string, string>();
            foreach (var parameter in parameters)
                Parameters.Add(parameter.Key, parameter.Value);
        }

        public RfcResultDto(byte[] data)
        {
            var dto = JsonConvert.DeserializeObject<RfcResultDto>(Encoding.UTF8.GetString(data));
            if (dto == null)
                throw new Exception("Can't deserialize data");
            IsSuccess = dto.IsSuccess;
            IsTimeout = dto.IsTimeout;
            Message = dto.Message;
            Parameters = dto.Parameters;
        }

        public byte[] Serialize()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}