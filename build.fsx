#r @"tools\FAKE\tools\FakeLib.dll"
#r @"FAKE.MongoDB\bin\Debug\FAKE.MongoDB.dll"

open Fake
open FAKE.MongoDB

let ConnectionString = "mongodb://localhost:27017/?connectTimeoutMS=30000&maxIdleTimeMS=600000"
let DbName = "tt"
let CollectionName = "test"
let file = "test.json";

MongoHelper.Eval(ConnectionString,DbName,"db.test.find({}).toArray()")
|> tracefn "Ret: %s"

let pipeline = [|
   "{$match:{}}";
   "{$limit:1}";
|]

MongoHelper.Export(ConnectionString,DbName,CollectionName,file,pipeline);
MongoHelper.Import(ConnectionString,DbName,"test1",file);


