using Flurl.Http.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Test.Http
{
    [TestFixture, Parallelizable]
    public class DefaultUrlEncodedSerializerTests
    {
        public class Grant
        {
            public string grant_type { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string refresh_token { get; set; }
            public string client_id { get; set; }
            public string client_secret { get; set; }
            public string scope { get; set; }
        }

        public class Grant_Types
        {
            public const string password = "password";
            public const string refresh_token = "refresh_token";
            public const string client_credentials = "client_credentials";
        }

        public class Grant_Scopes
        {
            public const string password = "openid offline_access";
            public const string client_credentials = "openid";
        }

        [Test]
        public void can_serialize_with_grant_class()
        {
            var serializer = new DefaultUrlEncodedSerializer();
            var result = serializer.Serialize(new Grant
            {
                grant_type = Grant_Types.password,
                username = "user",
                password = "password",
                client_id = "Client_Id",
                client_secret = "Client_Secret",
                scope = Grant_Scopes.password
            });

            Assert.AreEqual("grant_type=password&username=user&password=password&client_id=Client_Id&client_secret=Client_Secret&scope=openid+offline_access", result.ToString());
        }
    }
}
