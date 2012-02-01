using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;

namespace Zata.Web
{
    public class HttpMethodResult { public byte[] ByteStream { get; set; } }

    public class HttpResponseFilter : Stream
    {
        private Stream _sink;
        private long _position;

        public HttpResponseFilter(Stream sink)
        {
            _sink = sink;
        }

        // The following members of Stream must be overriden.
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return 0; }
        }

        public override long Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public override long Seek(long offset, System.IO.SeekOrigin direction)
        {
            return _sink.Seek(offset, direction);
        }

        public override void SetLength(long length)
        {
            _sink.SetLength(length);
        }

        public override void Close()
        {
            _sink.Close();
        }

        public override void Flush()
        {
            _sink.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _sink.Read(buffer, offset, count);
        }

        // The Write method actually does the filtering.
        public override void Write(byte[] buffer, int offset, int count)
        {
            string s = Encoding.UTF8.GetString(buffer);

            byte[] data = new byte[count];
            Buffer.BlockCopy(buffer, offset, data, 0, count);

            // <LinkPath specific changes to sample code>
            // This may not be pretty, but it works
            byte data1 = data[0], data2 = data[0], data3 = data[0], data4 = data[0];
            for (int i = 0; i < count; i++)
            {

                if (i > 4)
                { // once we're far enough in
                    data4 = data3;
                    data3 = data2;
                    data2 = data1;
                }
                data1 = data[i];

                // test for /en/ pattern
                if (data1 == Convert.ToByte('/'))
                {
                    if ((data4 == Convert.ToByte('/')) && (data3 == Convert.ToByte('e')) && (data2 == Convert.ToByte('n')))
                    {
                        // change it to /fr/
                        data[i - 1] = Convert.ToByte('r');
                        data[i - 2] = Convert.ToByte('f');
                    }
                }
                // test for _e_ pattern
                else if (data1 == Convert.ToByte('_'))
                {
                    if ((data3 == Convert.ToByte('_')) && (data2 == Convert.ToByte('e')))
                    {
                        // change it to _f_ 
                        data[i - 1] = Convert.ToByte('k');
                    }
                }
            }
            // </LinkPath specific changes to sample code>
            _sink.Write(data, 0, count);
        }

    } // LinkPathFilter
}
