using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FoodOntoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace FoodOntoWeb.Controllers
{
    public class FoodController : Controller
    {
        public IActionResult Index()
        {
            return View(new SearchViewModel());
        }
        [HttpPost]
        public IActionResult Index(SearchViewModel model)
        {
            // Load ontology file
            IGraph g = new Graph();
            var file = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "food-nt.owl");

            FileLoader.Load(g, file);

            // Spartql query
            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));
            query.Namespaces.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
            query.Namespaces.AddNamespace("uni", new Uri("http://www.semanticweb.org/nnt/ontologies/2019/10/food-nt#"));

            string type = "";
            string type_declare = "";
            if (model.Type != 0) type_declare = " ?food rdf:type ?type . ";
            if (model.Type == 1) type = " && ?type=uni:Ngu_coc ";
            if (model.Type == 2) type = " && ?type=uni:Khoai_cu ";
            if (model.Type == 3) type = " && ?type=uni:Hat_qua ";
            query.CommandText = "SELECT ?food WHERE " +
                "{ ?food uni:hasEnergyPer100g ?en ." +
                " ?food uni:hasProteinPer100g ?pr ." +
                " ?food uni:hasFatPer100g ?fat ." +
                " ?food uni:hasCarbPer100g ?ca ." +
                type_declare +
                "FILTER(?en>=" + model.Energy +
                "&& ?pr>=" + model.Protein +
                "&& ?fat>=" + model.Fat +
                "&& ?ca>=" + model.Carb + type +")}";

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery queryEx = parser.ParseFromString(query);

            // Execute query
            Object result = g.ExecuteQuery(queryEx);
            string data = "";
            if (result is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)result;
                INodeFormatter formatter = new TurtleFormatter(g);
                foreach (SparqlResult r in rset)
                {
                    String d;
                    INode n;
                    if (r.TryGetValue("food", out n))
                    {
                        switch (n.NodeType)
                        {
                            case NodeType.Uri:
                                d = ((IUriNode)n).Uri.Fragment;
                                break;
                            case NodeType.Literal:
                                d = ((ILiteralNode)n).Value;
                                break;
                            default:
                                throw new RdfOutputException("Unexpected Node Type");
                        }
                    }
                    else
                    {
                        d = String.Empty;
                    }
                    data = data + d.Replace("#", "").Replace("_", " ") + "\n";
                    model.Result.Add(d.Replace("#", "").Replace("_", " "));
                }
            }
            else data += "NO RESULT!";
            return PartialView("_Result", model.Result);
        }
    }
}