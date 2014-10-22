using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data.Test
{

    public class VendorCommentEntity 
    {
        public VendorCommentEntity()
        {
            this.UID = Guid.NewGuid();
            this.ParentID = Guid.Empty;
        }
        public Guid UID { get; set; }
        public Guid ParentID { get; set; }
        public string Category { get; set; }
        public string CategoryName { get; set; }
        public string VendorID { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public int Active { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class Entity
    {
        [Column(Name="PK")]
        public string PK { get; set; }
        public Guid FK { get; set; }
        public string Field1 { get; set; }
    }
}
