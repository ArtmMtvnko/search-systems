using VectorSpaceModel;

var docsPath = @"C:\Users\artem\Local Documents\University\Search Engine\VectorSpaceModel\docs\";

var model = new VSM(docsPath);
model.PrintVectors();