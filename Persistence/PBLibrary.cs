using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Relational.Octapus.Persistence
{
    public class PBLibrary
    {

        private string libraryName;
        private string libraryNameWithExtension;
        private string libraryResolvedPath;
        private string vssProjectPath;
        private string vssProductPath;
        private string vssLibPath;
        private string libFullPath;
        private string extLibPlusLibName;
        private string extLibPlusLibNameWithExt;
        private string localVssWorkingPath;
        private int libNamePosition;
        private bool isBinary;
        private bool isApplicationLibrary;
        private string localFullPath;



        private List<PBObject> objectsList;

        public PBLibrary(string libraryName, string libraryNameWithExtension, string libraryResolvedPath, string vssProjectPath, string vssProductPath,
                         string vssLibPath, string libFullPath, string extLibPlusLibName, string extLibPlusLibNameWithExt, string localVssWorkingPath,
                         int libNamePosition, bool isApplicationLibrary, string localFullPath)
        {
            this.libraryName = libraryName;
            this.libraryNameWithExtension = libraryNameWithExtension;
            this.libraryResolvedPath = libraryResolvedPath;
            this.isBinary = !this.libraryNameWithExtension.ToUpper().EndsWith("PBL");
            this.vssProjectPath = vssProjectPath;
            this.vssProductPath = vssProductPath;
            this.libFullPath = libFullPath;
            this.extLibPlusLibName = extLibPlusLibName;
            this.extLibPlusLibNameWithExt = extLibPlusLibNameWithExt;
            this.objectsList = new List<PBObject>();
            this.vssLibPath = vssLibPath;
            this.localVssWorkingPath = localVssWorkingPath;
            this.libNamePosition = libNamePosition;
            this.isApplicationLibrary = isApplicationLibrary;
            this.localFullPath = localFullPath;
        }

        public string LibraryName { get { return this.libraryName; } set { } }
        public string LibraryNameWithExtension { get { return this.libraryNameWithExtension; } set { } }
        public string LibraryResolvedPath { get { return this.libraryResolvedPath; } set { } }
        public string LibraryFullPath { get { return this.libFullPath; } set { } }
        public string ExtLibPlusLibName { get { return this.extLibPlusLibName; } set { } }
        public string ExtLibPlusLibNameWithExtension { get { return this.extLibPlusLibNameWithExt; } set { } }
        public string VssProjectPath { get { return this.vssProjectPath; } set { } }
        public string VssProductPath { get { return this.vssProductPath; } set { } }
        public string VssLibPath { get { return this.vssLibPath; } set { } }
        public string LocalVssWorkingPath { get { return this.localVssWorkingPath; } set { } }
        public int LibraryNamePosition { get { return this.libNamePosition; } set { } }
        public bool IsBinary { get { return this.isBinary; } set { } }
        public bool IsApplicationLibrary { get { return this.isApplicationLibrary; } set { } }
        public string LocalFullPath { get { return this.localFullPath; } set { } }
    }
}
