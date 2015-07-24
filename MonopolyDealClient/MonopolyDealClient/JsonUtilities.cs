/**
 * File: JsonUtilities.cs
 * Author: Joshua Wade
 * Date: 5-29-2014
 */
using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

public static class JsonUtilities
{
    /**
     * This function returns a serialized JSON string representation of the object provided.
     */
    public static string SerializeObjectToJSON(Object obj)
    {
        ASCIIEncoding encoding = new ASCIIEncoding();
        MemoryStream memStream = new MemoryStream();
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
        serializer.WriteObject(memStream, obj);
        string result = encoding.GetString(memStream.GetBuffer()).ToString();
        result = result.Substring(0, 1 + result.LastIndexOf('}'));
        return result;
    }

    /**
     * This function returns a deserialzed object given a serialized JSON string and the
     * object type given by type.
     */
    public static Object DeserializeObjectFromJSON(string s, Type type)
    {
        using (MemoryStream reader = new MemoryStream(Encoding.ASCII.GetBytes(s)))
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(type);
            try
            {
                return (Object)serializer.ReadObject(reader);
            }
            catch (Exception ex)
            {
                Exception boutTime = ex;
                Exception inner = ex.InnerException;
                return (Object)5;
            }
        }
    }
}
