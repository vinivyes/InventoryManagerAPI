using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using InventoryManagerAPI.Models;
using InventoryManagerAPI.Context;
using InventoryManagerAPI.Helpers;
using InventoryManagerAPI.Authorization;
using System.Text.Json;

namespace InventoryManagerAPI.Controllers
{    
     /// <summary>
     /// Controller for Category routes
     /// 
     /// Available Routes:
     /// [GET] /category/           | Gets a list of all stored Categories
     /// [GET] /category/{id}       | Get a Category with the specified {id}
     /// [POST] /category/          | Create a new Category
     /// [PUT] /category/{id}       | Update a Category with the specified {id}
     /// [DELETE] /category/{id}    | Deletes a Category with the specified {id}
     /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly InventoryContext _context;

        public CategoryController(InventoryContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a list of all stored Categories
        /// </summary>
        /// <returns>A list of Categories</returns>
        [HttpGet]
        [AuthorizeAction("/category/read")]
        public ActionResult<List<Category>> GetAll()
        {
            try
            {
                return _context.Categories
                               .ToList();
            }
            catch (Exception ex)
            {
                // Handle the exception as appropriate for your application
                return StatusCode(500, new
                {
                    code = "E200000",
                    message = Utils.Log(ex)
                });
            }
        }

        /// <summary>
        /// Get a category based on the {id}
        /// </summary>
        /// <returns>A category with the specified id</returns>
        [HttpGet("{id}")]
        [AuthorizeAction("/category/read")]
        public ActionResult<Category> GetById(int id)
        {
            try
            {
                Category category = _context.Categories
                                            .Where(c => c.id == id)
                                            .SingleOrDefault(c => c.id == id);

                if (category == null)
                {
                    return NotFound(new
                    {
                        code = "E200001",
                        message = "Category could not be found"
                    });
                }

                return category;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E200002",
                    message = Utils.Log(ex)
                });
            }
        }

        /// <summary>
        /// Creates a new category and saves it to the database.
        /// The category object is passed in as a parameter.
        /// The category object is then added to the database and changes are saved using the DbContext.
        /// A new object with the created category is returned in the response.
        /// If an exception is caught, a status code 500 is returned with an error message.
        /// </summary>
        /// <param name="category">A Category object containing category data</param>
        /// <returns>The newly created category</returns>
        [HttpPost]
        [AuthorizeAction("/category/write")]
        public ActionResult<Category> Create(Category category)
        {
            try
            {
                _context.Add(category);
                _context.SaveChanges();

                return category;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E200003",
                    message = Utils.Log(ex)
                });
            }
        }

        /// <summary>
        /// Updates an existing category with the given ID by replacing the existing properties with the new ones provided in the request body.
        /// If the given ID does not correspond to an existing category, a not found status code is returned.
        /// If the update operation is successful, a no content status code is returned.
        /// </summary>
        /// <param name="id">The ID of the category to be updated</param>  
        /// <param name="category">A Category object containing the updated category data</param>
        /// <returns>A status code indicating the outcome of the operation</returns>
        [HttpPut("{id}")]
        [AuthorizeAction("/category/write")]
        public IActionResult Update(int id, [FromBody] dynamic category)
        {
            try
            {
                //Retrieves the current state of the Category Object
                Category existingCategory = _context.Categories
                                             .SingleOrDefault((u) => u.id == id);

                if (existingCategory is null)
                    return NotFound(new
                    {
                        code = "E200004",
                        message = "Category has not been found"
                    });

                //Creates a dictionary of properties that should be updated.
                Dictionary<string, object> updateDict = JsonSerializer.Deserialize<Dictionary<string, object>>(category);

                //Apply property values to existing Category Object
                foreach (KeyValuePair<string, object> kvp in updateDict)
                {
                    if (kvp.Key == "id")
                        continue;


                    //Confirm if propert exists...
                    if (existingCategory.GetType().GetProperty(kvp.Key) is null)
                        return BadRequest(new
                        {
                            code = "E200005",
                            message = String.Format("Property '{0}' does not exist.", kvp.Key)
                        });


                    //Gets the type of the property being update - accounts for nullable properties
                    Type propertyType = existingCategory.GetType().GetProperty(kvp.Key).PropertyType;
                    propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                    //Converts the dynamic value into the correct type
                    object value = kvp.Value is null ?
                                                null :
                                                JsonSerializer.Deserialize(((JsonElement)kvp.Value).GetRawText(), propertyType);

                    //Dynamically sets the existing Category properties based on the received by the request - Unspecified properties remain unchanged.
                    existingCategory
                        .GetType()
                        .GetProperty(kvp.Key)
                        .SetValue(
                            existingCategory,
                            value,
                            null
                    );
                }

                _context.Categories
                        .Update(existingCategory);
                _context.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E200006",
                    message = Utils.Log(ex)
                });
            }
        }

        /// <summary>
        /// Deletes a category with the specified ID.
        /// If the given ID does not correspond to an existing category, a not found status code is returned.
        /// If the delete operation is successful, an OK status code is returned.
        /// </summary>
        /// <param name="id">The ID of the category to be deleted</param>
        /// <returns>A status code indicating the outcome of the operation</returns>
        [HttpDelete("{id}")]
        [AuthorizeAction("/category/delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                var category = _context.Categories.SingleOrDefault(c => c.id == id);

                if (category == null)
                {
                    return NotFound(new
                    {
                        code = "E200007",
                        message = "Category could not be found"
                    });
                }

                _context.Categories.Remove(category);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E200008",
                    message = Utils.Log(ex)
                });
            }
        }
    }
}