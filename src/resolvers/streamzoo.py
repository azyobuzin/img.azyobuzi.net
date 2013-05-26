# -*- coding: utf-8 -*-

import json
import urllib2

from resolvers import *

class Streamzoo(StoringResolver):
    @property
    def service_name(self):
        return "Streamzoo"

    @property
    def regex_str(self):
        return r"https?://(?:www\.)?streamzoo\.com/i/(\d+)(?:\?.*)?"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "streamzoo"
        columns = ["id", "thumb", "content"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return dict(zip(columns[1:], result))

        response = urllib2.urlopen("http://www.streamzoo.com/v1/item/" + param)
        j = json.load(response)

        thumb = j["thumbURL"]
        content = j["contentURL"]

        self.insert_all(cursor, table, (param, thumb, content))
        return dict(zip(columns[1:], (thumb, content)))

    def get_full(self, match):
        return self.work(match)["content"]

    def get_full_https(self, match):
        return self.get_full(match).replace("http://", "https://", 1)
    
    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return self.get_full_https(match)

    def get_thumb(self, match):
        return self.work(match)["thumb"]

    def get_thumb_https(self, match):
        return self.get_thumb(match).replace("http://", "https://", 1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
