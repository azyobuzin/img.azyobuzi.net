# -*- coding: utf-8 -*-

from resolvers import *

class My365(OpenGraphResolver):
    @property
    def service_name(self):
        return "My365"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?my365\.in/([\w\-]+)/p/(\d+)/?(?:\?.*)?$"

    def get_parameters(self, match):
        return {"username": match.group(1), "id": match.group(2)}

    def _work(self, param, cursor):
        table = "my365"
        columns = ["username", "id", "image"]
        result = self.select_one(cursor, table, columns[2:], {columns[0]: param["username"], columns[1]: param["id"]})
        if result:
            return result[0]

        req_uri = "http://my365.in/%(username)s/p/%(id)s" % param
        uri = self.read_og(req_uri, lambda res: res.geturl() == req_uri)

        if uri == False:
            raise PictureNotFoundError()

        self.insert_all(cursor, table, (param["username"], param["id"], uri))
        return uri

    def get_full(self, match):
        return self.work(match)

    def get_full_https(self, match):
        return self.get_full(match).replace("http://", "https://", 1)

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
