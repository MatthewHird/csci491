﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MvcPodium.ConsoleApp.Services
{
    public interface IIoUtilService
    {
        void WriteStringToFile(
            string outString,
            string outFilePath);
    }
}
