using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Model
{
    public class Detailitem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int MasterItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Qty { get; set; }

    }
}
