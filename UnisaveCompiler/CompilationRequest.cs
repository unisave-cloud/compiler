using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnisaveCompiler.Http;

namespace UnisaveCompiler
{
    public class CompilationRequest
    {
        /// <summary>
        /// What game are we working on
        /// </summary>
        [JsonProperty("game_id")]
        public string GameId { get; set; }

        /// <summary>
        /// What backend are we compiling
        /// </summary>
        [JsonProperty("backend_id")]
        public string BackendId { get; set; }

        /// <summary>
        /// List of backend files to download
        /// </summary>
        [JsonProperty("files")]
        public List<BackendFile> Files { get; set; }

        public struct BackendFile
        {
            [JsonProperty("path")] public string path;

            [JsonProperty("hash")] public string hash;
        }

        /// <summary>
        /// What version of Unisave Framework to compile against
        /// </summary>
        [JsonProperty("framework_version")]
        public string FrameworkVersion { get; set; }

        /// <summary>
        /// Should the compiler generate overflow checks?
        /// ('-checked+' option on the compiler)
        /// </summary>
        [JsonProperty("checked")]
        public bool Checked { get; set; } = false;

        /// <summary>
        /// Allow unsafe code
        /// ('-unsafe+' option on the compiler)
        /// </summary>
        [JsonProperty("unsafe")]
        public bool Unsafe { get; set; } = false;

        /// <summary>
        /// Version of the C# language to compile against
        /// </summary>
        [JsonProperty("lang_version")]
        public string LangVersion { get; set; } = "7.3";

        /// <summary>
        /// Allowed lang version attribute values
        /// </summary>
        public static readonly string[] AllowedLangVersions = {
            "1", "2", "3", "4", "5", "6",
            "7.0", "7.1", "7.2", "7.3",
            "8.0"
        };
        
        /// <summary>
        /// List of #define preprocessor symbols to be set
        /// </summary>
        [JsonProperty("define_symbols")]
        public List<string> DefineSymbols { get; set; } = new List<string>();

        /// <summary>
        /// Make sure the request contains valid data
        /// </summary>
        public void Validate()
        {
            if (!ValidateRegex(GameId, "^[a-zA-Z0-9]+$"))
                throw new ValidationException(
                    "Game ID has invalid format"
                );
            
            if (!ValidateRegex(BackendId, "^[a-zA-Z0-9]+$"))
                throw new ValidationException(
                    "Backend ID has invalid format"
                );
            
            var paths = new HashSet<string>();
            
            foreach (var file in Files)
            {
                if (file.path.StartsWith("/"))
                    throw new ValidationException(
                        "Backend file paths must be relative"
                    );
                
                if (file.path.Contains("\\"))
                    throw new ValidationException(
                        "Backend file paths must be in the unix style"
                    );
                
                if (file.path.Length > 2048)
                    throw new ValidationException(
                        "Backend file paths must be at most 2048 chars long"
                    );
                
                if (file.hash.Length > 64)
                    throw new ValidationException(
                        "Backend file hashes must be at most 64 chars long"
                    );
                
                if (!ValidateRegex(file.hash, "^[a-zA-Z0-9]+$"))
                    throw new ValidationException(
                        "File hashes cannot contain slashes"
                    );

                if (paths.Contains(file.path))
                    throw new ValidationException(
                        $"File has been provided multiple times: {file.path}"
                    );
                
                paths.Add(file.path);
            }
            
            if (!ValidateRegex(FrameworkVersion, "^[a-zA-Z0-9_\\.\\-]+$"))
                throw new ValidationException(
                    "Framework version has invalid format"
                );
            
            if (!AllowedLangVersions.Contains(LangVersion))
                throw new ValidationException(
                    "Given language version is not allowed"
                );
            
            if (DefineSymbols == null)
                throw new ValidationException(
                    "Define symbols cannot be null"
                );

            foreach (string symbol in DefineSymbols)
            {
                if (!ValidateRegex(symbol, "^[a-zA-Z0-9_]+$"))
                    throw new ValidationException(
                        $"Define symbol '{symbol}' is invalid."
                    );
            }
        }

        private bool ValidateRegex(string subject, string pattern)
        {
            var regex = new Regex(pattern);
            return regex.IsMatch(subject);
        }
    }
}