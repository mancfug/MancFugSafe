namespace Shared

type NextEvent =
    { Name : string
      Attendees: int }

type Meetup =
    { Name : string
      Details : string
      NextEvent : NextEvent }
