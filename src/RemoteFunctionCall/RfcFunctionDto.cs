using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RemoteFunctionCall
{
    public class RfcFunctionDto
    {
        public string FunctionName { get; set; } = "";
        public Dictionary<string, string> Parameters { get; set; } = null!;

        public RfcFunctionDto()
        {
        }

        public RfcFunctionDto(string functionName, params KeyValuePair<string, string>[] parameters)
        {
            FunctionName = functionName;
            Parameters = new Dictionary<string, string>();
            foreach (var parameter in parameters)
                Parameters.Add(parameter.Key, parameter.Value);
        }

        public RfcFunctionDto(byte[] data)
        {
            var dto = JsonConvert.DeserializeObject<RfcFunctionDto>(Encoding.UTF8.GetString(data));
            if (dto == null)
                throw new Exception("Can't deserialize data");
            FunctionName = dto.FunctionName;
            Parameters = dto.Parameters;
        }

        public byte[] Serialize()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}