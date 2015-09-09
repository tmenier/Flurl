using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flurl.Http.Configuration
{
    public interface ISerializer
    {
	    string Serialize(object obj);
		T Deserialize<T>(string s);
		T Deserialize<T>(Stream stream);
    }
}
