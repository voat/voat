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

namespace Voat.Domain.Models
{
    //HACK: THis class needs to inherit from ApiSubverseInfo as to reduced duplication
    //UPDATE: Sets removed from api/v1/ spec
    public class SetInformation
    {
        //    public string Name { get; set; }
        //    public string Title { get; set; }
        //    public string Description { get; set; }
        //    public DateTime CreationDate { get; set; }
        //    public int SubscriberCount { get; set; }
        //    public Nullable<bool> RatedAdult { get; set; }
        //    public string Sidebar { get; set; }
        //    public string Type { get; set; }
    }
}
