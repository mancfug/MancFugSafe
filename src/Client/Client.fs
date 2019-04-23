module Client

open Elmish
open Elmish.React
open Fable.FontAwesome
open Fable.FontAwesome.Free
open Fable.Helpers
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch
open Fulma
open Shared
open Thoth.Json

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model =
    { Meetup : Meetup option }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg = InitialMeetupLoaded of Result<Meetup, exn>

let initialMeetup = fetchAs<Meetup> "/api/init" (Decode.Auto.generateDecoder())

// defines the initial state and initial command (= side-effect) of the application
let init() : Model * Cmd<Msg> =
    let initialModel = { Meetup = None }
    let loadCountCmd = Cmd.ofPromise initialMeetup [] (Ok >> InitialMeetupLoaded) (Error >> InitialMeetupLoaded)
    initialModel, loadCountCmd

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match msg with
    | InitialMeetupLoaded(Ok initialMeetup) ->
        let nextModel = { Meetup = Some initialMeetup }
        nextModel, Cmd.none
    | _ -> currentModel, Cmd.none

let spinner = Icon.icon [ Icon.Size IsLarge ] [ Fa.i [ Fa.Solid.Spinner; Fa.Pulse; Fa.Size Fa.Fa3x ] [] ]

let safeComponents name =
    let components =
        span [ ClassName "main-links"]
                [ a [ Href "https://saturnframework.github.io" ] [ str "Saturn" ]
                  str ", "
                  a [ Href "http://fable.io" ] [ str "Fable" ]
                  str ", "
                  a [ Href "https://elmish.github.io/elmish/" ] [ str "Elmish" ]
                  str ", "
                  a [ Href "https://fulma.github.io/Fulma" ] [ str "Fulma" ]
                  str ", "
                  a [ Href "https://dansup.github.io/bulma-templates/" ] [ str "Bulma\u00A0Templates" ] ]
    p [] [ strong [] [ str name ]
           str " powered by: "
           components ]

let navBrand =
    // Navbar.Brand.div [ ]
    //     [ Navbar.Item.a
    //         [ Navbar.Item.Props [ Href  ] ]
    //         [
    img [ Src "./FUG.jpeg"
          Alt "Logo"
          Href "https://www.meetup.com/Manchester-F-User-Group/"
          Style [ Width "100px"
                  Height "100px" ] ] //] ]


let navMenu =
    Navbar.menu []
        [ Navbar.End.div []
              [ Navbar.Item.div []
                    [ Button.a [ Button.Color IsWhite
                                 Button.IsOutlined
                                 Button.Size IsSmall
                                 Button.Props [ Href "https://github.com/mancfug/MancFugSafe" ] ]
                          [ Icon.icon [] [ Fa.i [ Fa.Brand.Github ] [] ]
                            span [] [ str "View Source" ] ] ] ] ]

let card icon heading body =
    Column.column [ Column.Width(Screen.All, Column.Is4) ]
        [ Card.card [] [ Card.image [ Modifiers [ Modifier.TextAlignment(Screen.All, TextAlignment.Centered) ] ]
                             [ Icon.icon [ Icon.Size IsMedium
                                           Icon.Props [ Style [ MarginTop "15px" ] ] ]
                                   [ Fa.i [ icon
                                            Fa.IconOption.Size Fa.Fa2x ] [] ] ]
                         Card.content [] [ Content.content [] [ h4 [] [ str heading ]
                                                                p [] [ body ] ] ] ] ]

let features (v: Meetup) =

    Columns.columns [ Columns.CustomClass "features" ]
        [ card Fa.Solid.CodeBranch "Meetup Info" (div[ (React.Props.DangerouslySetInnerHTML({ __html = v.Details })) ] [])

          card Fa.Solid.Handshake "Next Event"
              (str (sprintf "Name: %s, Attendees: %i" v.NextEvent.Name v.NextEvent.Attendees))

          card Fa.Solid.Rocket "Where to Find us"

            ( div [ (React.Props.DangerouslySetInnerHTML({ __html = """<iframe width='100%' height='100%' id='mapcanvas' src='https://maps.google.com/maps?q=17%20Marble%20St,%20Manchester&amp;t=&amp;z=16&amp;ie=UTF8&amp;iwloc=&amp;output=embed' frameborder='0' scrolling='no' marginheight='0' marginwidth='0'><div class="zxos8_gm"><a href="https://themesort.com/category/agency-themes">for an agency</a></div><div style='overflow:hidden;'><div id='gmap_canvas' style='height:100%;width:100%;'></div></div><div><small>Powered by <a href="https://www.embedgooglemap.co.uk">Embed Google Map</a></small></div></iframe>""" })) ][])
          ]

let intro =
    Column.column [ Column.CustomClass "intro"
                    Column.Width(Screen.All, Column.Is8)
                    Column.Offset(Screen.All, Column.Is2) ]
        [ h2 [ ClassName "title" ] [ str "Perfect for newcomers and pros alike!" ]
          br []
          p [ ClassName "subtitle" ]
            [
                str "Come to learn about the Functional First dotnet language, then use it to take over the world";
                br [];
                br [];
                img [ Src "https://media3.giphy.com/media/LZFgI1B26kzG8/giphy.gif?cid=790b76115cbe34e95055574d3677ec69"; Alt "Take over the world"; Style [ Width "450px"; Height "450px" ] ] ] ]

let tile title subtitle content =
    let details =
        match content with
        | Some c -> c
        | None -> nothing
    Tile.child [] [ Notification.notification [ Notification.Color IsWhite ] [ Heading.p [] [ str title ]

                                                                               Heading.p [ Heading.IsSubtitle ]
                                                                                   [ str subtitle ]
                                                                               details ] ]

let content txts =
    Content.content [] [ for txt in txts -> p [] [ str txt ] ]

let footerContainer =
    Container.container []
        [ Content.content [ Content.Modifiers [ Modifier.TextAlignment(Screen.All, TextAlignment.Centered) ] ]
              [ p [ ClassName "footer-links"] [ safeComponents "Created using SAFE Template" ]

                p []
                    [ a [ Href "https://github.com/SAFE-Stack/SAFE-template" ]
                          [ Icon.icon [] [ Fa.i [ Fa.Brand.Github ] [] ] ] ] ] ]

let view (model : Model) (dispatch : Msg -> unit) =
    div [] [ yield Hero.hero [ Hero.IsMedium; Hero.IsBold ]
                       [ Hero.head [] [ Navbar.navbar [] [ Container.container [] [ navBrand; navMenu ] ] ]
                         Hero.body []
                             [ Container.container
                                   [ Container.Modifiers [ Modifier.TextAlignment(Screen.All, TextAlignment.Centered) ] ]
                                   [ Heading.p [] [ str "Manchester F# User Group" ]
                                     Heading.p [ Heading.IsSubtitle; Heading.CustomClass "main-links"] [ safeComponents "Manchester F# User Group" ] ] ] ]
             yield Container.container []
                   (match model.Meetup with
                   | Some v ->
                        [
                            features v
                            intro
                        ]
                   | None ->
                        [
                            div [ ClassName "sandbox" ]
                                [ Tile.ancestor [ ]
                                    [ Tile.parent [ Tile.Size Tile.Is12 ]
                                        [ tile "Loading.." "Please wait" (Some spinner)  ]
                                    ] ]
                        ])
             yield footer [ ClassName "footer" ] [ footerContainer ] ]
#if DEBUG

open Elmish.Debug
open Elmish.HMR
#endif


Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif

|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif

|> Program.run
