using DAL.Model;
using DAL.Repositeries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Test.ViewModel;

namespace Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ILogger<CategoriesController> _logger;
        private readonly RepositeryContext _dbContext;

        public CategoriesController(ILogger<CategoriesController> logger, RepositeryContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        [HttpPost, Route("Category")]
        public IActionResult Category(CategoryViewModel model)
        {

            if (model == null || string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Type))
            {
                return BadRequest("Invalid input data.");
            }
            var cat = new Category();
            var userdata = _dbContext.categories.FirstOrDefault(x => x.Name == model.Name);
            var userdatas = _dbContext.categories.FirstOrDefault(x => x.Name == model.ChildCatName);
            if ((model.Type == "P" && userdata != null) || (model.Type != "P" && userdatas != null))
            {
                return BadRequest(model.Type == "P"
                    ? "Category already exists in the system."
                    : "Given parent category does not exist in the system.");
            }
            cat.ParentId = model.Type == "P" ? -1 : userdata.Id;
            cat.Name = model.Type == "P" ? model.Name : model.ChildCatName;
            _dbContext.categories.Add(cat);
            _dbContext.SaveChanges();
            return Ok(model);
        }

        [Authorize, HttpGet, Route("GetCategory")]
        public IActionResult GetCategory()
        {
            var userdata = _dbContext.categories.ToList();
            List<CategorListViewModel> model = new List<CategorListViewModel>();
            foreach (var category in userdata)
            {
                CategorListViewModel viewModel = new CategorListViewModel
                {
                    id = category.Id,
                    Name = category.Name,
                    ParentId = category.ParentId
                };
                model.Add(viewModel);
            }
            return Ok(model);
        }

        [Authorize, HttpGet, Route("GetParentCategory")]
        public IActionResult GetParentCategory()
        {
            var userdata = (from x in _dbContext.categories
                            where x.ParentId == -1
                            select new ParentCategoryViewmodel
                            {
                                Id = x.Id,
                                Name = x.Name
                            }).ToList();
            List<ParentCategoryViewmodel> model = new List<ParentCategoryViewmodel>();
            foreach (var name in userdata)
            {
                model.Add(new ParentCategoryViewmodel
                {
                    Id = name.Id,
                    Name = name.Name
                });
            }
            return Ok(model);
        }
        [HttpGet, Route("GetChildCategory")]
        public IActionResult GetChildCategory(string name)
        {
            var parent = _dbContext.categories.Where(x => x.Name == name).Select(x => x.Id).First();
            var userdata = (from x in _dbContext.categories
                            where x.ParentId == parent
                            select new ChildCategory
                            {
                                Id = x.Id,
                                ChildCatName = x.Name
                            }).ToList();

            List<ChildCategory> model = new List<ChildCategory>();
            foreach (var category in userdata)
            {
                ChildCategory viewModel = new ChildCategory
                {
                    Id = category.Id,
                    ChildCatName = category.ChildCatName
                };
                model.Add(viewModel);
            }
            return Ok(model);
        }

        [HttpPost, Route("UpdateCategory")]
        public IActionResult UpdateCategory([FromBody] UpdateCategorViewModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Name))
            {
                return BadRequest("Invalid category data.");
            }
            var userdata = _dbContext.categories.FirstOrDefault(x => x.Id == model.id);

            var exitcat = _dbContext.categories.ToList();

            if (userdata == null)
            {
                return NotFound("Category not found.");
            }
            foreach (var cat in exitcat)
            {
                if (cat.Name == model.Name && cat.Name == model.Name)
                {
                    return BadRequest("Category Same Name already register");

                }
            }

            userdata.Name = model.Name;

            _dbContext.SaveChanges();

            return Ok(userdata);
        }

    }
}
