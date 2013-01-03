/*  
Copyright (C) 2013 Alan Pipitone
    
Alexa is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Alexa is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Alexa.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;

namespace Alexa.Utilities
{
    class CryptoUtils
    {
        private static PasswordDeriveBytes _secretKey;
        private static string _decryptKey = "a!ss£35s2s@3";
        private static byte[] _salt;
        private static bool _debugLogLevel = false;
        private static bool _warningLogLevel = false;

        /// <summary>
        /// Init the CryptoUtils class
        /// </summary>
        public static void Init()
        {
            try
            {
                //add other chars to _decryptKey
                _decryptKey = _decryptKey + "434fdrerd|+55689)]?*";

                //we need _salt to generate the secret key
                _salt = Encoding.ASCII.GetBytes(_decryptKey.Length.ToString());

                //set the loglevel
                if (ConfigUtils.ErrorLevel == LogUtils.ErrorLevel.Debug && ConfigUtils.LogIsEnabled == true)
                {
                    _debugLogLevel = true;
                    _warningLogLevel = true;
                }
                else if (ConfigUtils.ErrorLevel == LogUtils.ErrorLevel.Warning && ConfigUtils.LogIsEnabled == true)
                {
                    _warningLogLevel = true;
                }

            }
            catch (Exception ex)
            {
                LogUtils.Write(ex);
                Program.Finish(true);
            }
        }

        /// <summary>
        /// Decrypt a string
        /// </summary>
        /// <param name="cipherString">text to decrypt</param>
        /// <returns>returns plain text</returns>
        public static string DecryptString(string cipherString)
        {

            // We use RijndaelManaged object to decrypt the data.
            RijndaelManaged aesObj = null;

            string plaintext = null;

            try
            {
                aesObj = new RijndaelManaged();

                System.Text.Encoding enc = System.Text.Encoding.ASCII;
                byte[] cipherText = Convert.FromBase64String(cipherString);

                // create the secret key
                _secretKey = new PasswordDeriveBytes(_decryptKey, _salt);

                //declare the decryptor
                ICryptoTransform decryptor = aesObj.CreateDecryptor(_secretKey.GetBytes(32), _secretKey.GetBytes(16));

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))

                            //get the plain text
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            catch
            {
                if (_debugLogLevel)
                    LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "The string \"" + cipherString + "\" cannot be decrypted.");
            }
            finally
            {
                // Dispose the aesObj object.
                if (aesObj != null)
                    aesObj.Clear();
            }

            return plaintext;
        }

    }
}
