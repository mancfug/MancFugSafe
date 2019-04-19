open FSharp.Control.Tasks.V2
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Saturn
open Shared
open System.IO
open System.Threading.Tasks

let tryGetEnv =
    System.Environment.GetEnvironmentVariable
    >> function
    | null
    | "" -> None
    | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port =
    "SERVER_PORT"
    |> tryGetEnv
    |> Option.map uint16
    |> Option.defaultValue 8085us

let getInitCounter() : Task<Counter> = task { return { Value = Some 42 } }
let action s next ctx =
    task { let! counter = getInitCounter()
           return! json counter next ctx }

let action2 (x : string) next ctx =
    task {
        let queueResult = Ok x
        let getResult =
            match queueResult with
            | Ok queueUrl -> json { QueueUrl = queueUrl }
            | Error e -> RequestErrors.badRequest (text e)
        return! getResult next ctx
    }

let webApp =
    router {
        getf "/api/init/%s" action
        getf "/api/test/%s" action2
    }

let app =
    application {
        url ("http://0.0.0.0:" + port.ToString() + "/")
        use_router webApp
        memory_cache
        use_static publicPath
        use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
        use_gzip
    }

run app
