# -*- coding: utf-8 -*-

import json
import urllib2

from resolvers import *

class CloudApp(StoringResolver):
    @property
    def service_name(self):
        return "CloudApp"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?cl\.ly/(image/\w+|\w+)/?(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "cloudapp"
        columns = ["id", "remote", "thumbnail"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return dict(zip(columns[1:], result))

        try:
            response = urllib2.urlopen(urllib2.Request("http://cl.ly/" + param, headers={"Accept": "application/json"}))
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e

        j = json.load(response)

        remote = j["remote_url"]
        thumbnail = j["thumbnail_url"]

        self.insert_all(cursor, table, (param, remote, thumbnail))
        return dict(zip(columns[1:], (remote, thumbnail)))

    def get_full(self, match):
        return self.work(match)["remote"]

    def get_full_https(self, match):
        return self.get_full(match).replace("http://", "https://", 1)

    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return self.get_full_https(match)

    def get_thumb(self, match):
        return self.work(match)["thumbnail"]

    def get_thumb_https(self, match):
        return self.get_thumb(match).replace("http://", "https://", 1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
