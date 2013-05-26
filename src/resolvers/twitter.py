# -*- coding: utf-8 -*-

import json

from resolvers import *
from resolvers.private_constant import *

class Twitter(StoringResolver):
    @property
    def service_name(self):
        return "Twitter"

    @property
    def regex_str(self):
        return r"^(?:(https?://(?:\w+\.twimg\.com/media|p\.twimg\.com)/[\w\-]+\.\w+)(?::\w+)?|https?://(?:www\.)?twitter\.com/(?:#!/)?\w+/status(?:es)?/(\d+)/photo/1(?:/(?:\w+/?)?)?)(?:\?.*)?$"
    
    def get_parameters(self, match):
        return match.group(2)

    def get_media(self, match):
        media = match.group(1)
        return media if media else self.work(match)

    def _work(self, param, cursor):
        table = "twitter"
        columns = ["id", "media"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return result[0]

        resp, content = twitter_client.request("https://api.twitter.com/1.1/statuses/show.json?include_entities=true&id=" + param, "GET")

        if resp["status"] == "404":
            raise PictureNotFoundError()
        elif resp["status"] != "200":
            raise Exception("Twitter returned " + resp["status"])

        entities = json.loads(content)["entities"]

        if "media" not in entities:
            raise IsNotPictureError()

        media = entities["media"][0]["media_url"]

        self.insert_all(cursor, table, (param, media))
        return media

    def get_full(self, match):
        return self.get_media(match) + ":orig"

    def get_full_https(self, match):
        return self.get_full(match).replace("http://", "https://")

    def get_large(self, match):
        return self.get_media(match)

    def get_large_https(self, match):
        return self.get_large(match).replace("http://", "https://")

    def get_thumb(self, match):
        return self.get_media(match) + ":thumb"

    def get_thumb_https(self, match):
        return self.get_thumb(match).replace("http://", "https://")

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
