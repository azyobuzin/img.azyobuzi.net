# -*- coding: utf-8 -*-

import re

from resolvers import *

class FxCamera(TwitterCardResolver):
    uri_regex = re.compile(r"^http://img\.fxc\.am/scaled/anon/(\w+)+$")

    @property
    def service_name(self):
        return "FxCamera"

    @property
    def regex_str(self):
        return r"^https?://fxc\.am/p/([\w\-]+)/?(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "fxcamera"
        columns = ["id", "scaled"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return result[0]

        try:
            scaled = self.read_twitter_card("http://fxc.am/p/" + param)
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e

        self.insert_all(cursor, table, (param, scaled))
        return scaled

    def get_full(self, match):
        return self.work(match)

    def get_full_https(self, match):
        return self.get_full(match).replace("http://", "https://", 1)

    def get_large(self, match):
        return re.sub(self.uri_regex, r"http://img.fxc.am/thumbs/anon/\1/keep640", self.get_full(match))

    def get_large_https(self, match):
        return re.sub(self.uri_regex, r"https://img.fxc.am/thumbs/anon/\1/keep640", self.get_full(match))

    def get_thumb(self, match):
        return self.get_large(match)

    def get_thumb_https(self, match):
        return self.get_large_https(match)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
