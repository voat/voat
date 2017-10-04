#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Voat.Common;

namespace Voat.Models.ViewModels
{
    public enum ErrorType
    {
        Default,
        SubverseDisabled,
        SubvereExists,
        Unauthorized,
        TheOthers,
        SubverseNotFound,
        NotFound,
        ThrowException
    }
    public class ErrorViewModel
    {
        public ErrorViewModel() { }

        public ErrorViewModel(string title, string description, string footer)
        {
            this.Title = title;
            this.Description = description;
            this.Footer = footer;
        }

        public bool UseLayout { get; set; } = true;
        public string Title { get; set; } = "Whoops!";
        public string Description { get; set; } = "Well this is embarrassing. Something went wrong and let&#39;s face it, nobody&#39;s happy about it.\nWe&#39;ll dispatch our monstersquad to take a look at it right away!";
        public string Footer { get; set; } = "Thank you for being a chap.";


        public static ErrorViewModel GetErrorViewModel(HttpStatusCode code)
        {
            switch (code)
            {
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.Unauthorized:
                    return GetErrorViewModel(ErrorType.Unauthorized);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(code), "The status code you provided is not handled. Update your code.");
                    break;
            }
        }
        public static ErrorViewModel GetErrorViewModel(ErrorType type)
        {
            var errorModel = new ErrorViewModel();
                switch (type)
                {
                    case ErrorType.SubverseDisabled:
                        errorModel.Title = "Subverse Disabled";
                        errorModel.Description = @"The subverse you were looking for has been disabled and is no longer accessible
                                                    <p>If you are a moderator of this subverse you may contact Voat for information regarding why this subverse is no longer active</p>";
                        //errorModel.FooterMessage = "";
                        break;
                    case ErrorType.SubvereExists:
                        errorModel.Title = "Mesosad!";
                        errorModel.Description = "The subverse you were trying to create already exists. Sorry about that. Try another name?";
                        errorModel.Footer = "Care to go back and try another name?";
                        break;
                case ErrorType.Unauthorized:
                        errorModel.Title = "Hold on there fella!";
                        errorModel.Description = "You were not supposed to be poking around here.";
                        errorModel.Footer = "How about you stop poking around? :)";
                        break;
                case ErrorType.TheOthers:
                        //errorModel.ShowGoat = false;
                        errorModel.Title = "The Others";
                        errorModel.Description = "<span style=\"font-size:10em;font-family:Verdana;\">O_o</span><p>This is not the place you think it is</p>";
                        errorModel.Footer = "Hmmm...";
                        break;
                case ErrorType.SubverseNotFound:
                        //errorModel.ShowGoat = false;
                        errorModel.Title = "Whoops!";
                        errorModel.Description = "The subverse you were looking for could not be found. Are you sure you typed it right? Also, I may have umm... eaten it.";
                        errorModel.Footer = "Pushing F5 repeatedly will not help";
                        break;
                case ErrorType.NotFound:
                        //errorModel.ShowGoat = false;
                        errorModel.Title = "Whoops!";
                        errorModel.Description = "The thing you were looking for could not be found. Are you sure you typed it right? Also, I may have eaten it.";
                        errorModel.Footer = "Pushing F5 repeatedly will not help";
                        break;
                    case ErrorType.ThrowException:
                        throw new InvalidOperationException("I was told to do this");
                        break;
                }
            
            return errorModel;
        }

    }
}
