using System.Collections.Generic;

namespace UnisaveCompiler
{
    public class CompilationRequest
    {
        public string GameId { get; set; }
        
        public string BackendId { get; set; }
        
        public List<BackendFile> Files { get; set; }
        
        public string FrameworkVersion { get; set; }

        public struct BackendFile
        {
            public string path;
            public string hash;
        }
    }
}