open FSharp.Control.Tasks.V2
open FSharp.Data
open Giraffe
open Giraffe.HttpStatusCodeHandlers
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open Saturn
open Shared
open System.IO
open System.Net.Http
open System.Reflection
open System.Threading.Tasks

type MeetupGroup = JsonProvider<"""./exampleMeetup.json""">

type MeetupSettings =
    { API : string }

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

let meetupClient() =
    let client = new HttpClient()
    client.BaseAddress <- System.Uri("https://api.meetup.com")
    client

let getStringResponse (client : HttpClient) (path : string) =
    task {
        let! result = client.GetAsync(path)
        result.EnsureSuccessStatusCode() |> ignore
        return! result.Content.ReadAsStringAsync()
    }

let getGroup (ctx : HttpContext) =
    task {
        try
            let settings = ctx.GetService<MeetupSettings>()
            use client = meetupClient()
            let meetupRequestPath = sprintf "Manchester-F-User-Group?&sign=true&photo-host=public&key=%s" settings.API
            let! meetupContent = meetupRequestPath |> getStringResponse client
            let parseMeetupResult = MeetupGroup.Parse meetupContent
            return { Name = parseMeetupResult.Name
                     Details = parseMeetupResult.Description
                     NextEvent = { Name = parseMeetupResult.NextEvent.Name; Attendees = parseMeetupResult.NextEvent.YesRsvpCount } }
                   |> Ok
        with e -> return Error e
    }

let webApp =
    router { get "/api/init" (fun next ctx ->
                 task {
                     let! result = getGroup ctx
                     return! match result with
                             | Ok v -> json v next ctx
                             | Error e -> json e next ctx
                 }) }

let configureServices (services : IServiceCollection) =
    let serviceProvider = services.BuildServiceProvider()
    let settings = serviceProvider.GetService<IConfiguration>()
    let meetupKey = settings.GetValue<string>("Meetup:API")
    services.AddSingleton({ API = meetupKey })

let app =
    application {
        url ("http://0.0.0.0:" + port.ToString() + "/")
        use_router webApp
        memory_cache
        use_static publicPath
        use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
        use_gzip
        service_config configureServices
    }

let config (webHost : WebHostBuilderContext) (cb : IConfigurationBuilder) =
    let env = webHost.HostingEnvironment
    if env.IsDevelopment() then cb.AddUserSecrets(Assembly.GetExecutingAssembly()) |> ignore

run (app.ConfigureAppConfiguration(config))
