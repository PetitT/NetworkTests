using System.Collections.Generic;

namespace FishingCactus.Platform
{
    public class MonoFile : IFile
    {
        public bool Delete( string file_path )
        {
            try
            {
                System.IO.File.Delete( file_path );
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Exists( string file_path )
        {
            try
            {
                return System.IO.File.Exists( file_path );
            }
            catch
            {
                return false;
            }
        }

        public bool ReadAllText( string file_path, out string contents )
        {
            try
            {
                contents = System.IO.File.ReadAllText( file_path );
                return true;
            }
            catch
            {
                contents = string.Empty;
                return false;
            }
        }

        public bool WriteAllText( string file_path, string contents )
        {
            try
            {
                System.IO.File.WriteAllText( file_path, contents );
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class MonoDirectory : IDirectory
    {
        public bool Create( string directory_path )
        {
            try
            {
                System.IO.Directory.CreateDirectory( directory_path );
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Delete( string directory_path )
        {
            try
            {
                System.IO.Directory.Delete( directory_path );
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Exists( string directory_path )
        {
            try
            {
                return System.IO.Directory.Exists( directory_path );
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable< string > GetFiles( string parent_directory_path )
        {
            return System.IO.Directory.GetFiles( parent_directory_path );
        }
        
        public IEnumerable< string > GetDirectories( string parent_directory_path )
        {
            return System.IO.Directory.GetDirectories( parent_directory_path );
        }
    }

    public class MonoIO : IIO
    {
        public IFile File => file;
        public IDirectory Directory => directory;

        private IFile file = new MonoFile();
        private IDirectory directory = new MonoDirectory();
    }
}