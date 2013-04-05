# -*- coding: utf-8 -*-

import re
from resolvers import *

class Cameran(OpenGraphResolver):
    @property
    def service_name(self):
        return "cameran"

    @property
    def regex_str(self):
        return r"^http://cameran.in/posts/get/v1/(\w+)(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "cameran"
        columns = ["id", "image"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return result

        req_uri = "http://cameran.in/posts/get/v1/" + param
        uri = self.read_og(req_uri, lambda res: res.geturl() == req_uri)

        if uri == False:
            raise PictureNotFoundError()

        self.insert_all(cursor, table, (param, uri))
        return uri

    def get_full(self, match):
        return self.get_full_https(match).replace("https://", "http://", 1)

    def get_full_https(self, match):
        return self.work(match)

    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return self.get_full_https(match)

    def get_thumb(self, match):
        return self.get_full(match)

    def get_thumb_https(self, match):
        return self.get_full_https(match)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
