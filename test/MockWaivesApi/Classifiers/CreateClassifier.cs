using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace MockWaivesApi.Classifiers
{
    public class CreateClassifier : ControllerBase
    {
        [HttpPost, Route("/classifiers/{name}")]
        public IActionResult Execute([FromRoute] string name)
        {
            return Created($"/classifiers/{name}", new
            {
                _links = new Dictionary<string, string>
                {
                    { "classifier:add_samples_from_zip", "" }
                }
            });
        }
    }
}
