# -*- coding: utf-8 -*-

from resolvers import *

class Cameran(OpenGraphResolver):
    @property
    def service_name(self):
        return "cameran"

    @property
    def regex_str(self):
        return r"^http://cameran\.in/(?:posts/get/v1/(\w+)|p/v1/(\w+))/?(?:\?.*)?$"

    def get_parameters(self, match):
        return {"old": match.group(1), "new": match.group(2)}

    def _work(self, param, cursor):
        new = param["new"] != None
        id = param["new"] if new else param["old"]

        table = "cameran"
        columns = ["id", "image"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: id})
        if result:
            return result[0]

        req_uri = ("http://cameran.in/p/v1/" if new else "http://cameran.in/posts/get/v1/") + id
        uri = self.read_og(req_uri, lambda res: res.geturl() == req_uri)

        if uri == False:
            raise PictureNotFoundError()

        self.insert_all(cursor, table, (id, uri))
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
