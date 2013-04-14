# -*- coding: utf-8 -*-

import re

from resolvers import *

class Path(OpenGraphResolver):
    uri_regex = re.compile(r"^(https://[a-z0-9/_\-\.]+/)original(\.\w+)$", re.IGNORECASE)

    @property
    def service_name(self):
        return "Path"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?path\.com/p/(\w+)(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "path"
        columns = ["id", "prefix", "extension"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return dict(zip(columns[1:], result))

        try:
            uri = self.read_og("https://www.path.com/p/" + param)
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e

        uri_match = self.uri_regex.match(uri)
        if uri_match:
            prefix = uri_match.group(1)
            extension = uri_match.group(2)
        else:
            raise IsNotPictureError()

        self.insert_all(cursor, table, (param, prefix, extension))
        return dict(zip(columns[1:], (prefix, extension)))

    def get_full(self, match):
        return self.get_full_https(match).replace("https://", "http://", 1)

    def get_full_https(self, match):
        result = self.work(match)
        return result["prefix"] + "original" + result["extension"]

    def get_large(self, match):
        return self.get_large_https(match).replace("https://", "http://", 1)

    def get_large_https(self, match):
        result = self.work(match)
        return result["prefix"] + "2x" + result["extension"]

    def get_thumb(self, match):
        return self.get_thumb_https(match).replace("https://", "http://", 1)

    def get_thumb_https(self, match):
        result = self.work(match)
        return result["prefix"] + "1x" + result["extension"]

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
