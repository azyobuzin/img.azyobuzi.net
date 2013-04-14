# -*- coding: utf-8 -*-

from resolvers import *

class Imgur(Resolver):
    @property
    def service_name(self):
        return "imgur"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.|i\.)?imgur\.com/(?:gallery/)?(\w+)(?:\.\w+)?/?(?:\?.*)?$"

    #拡張子関係なく元のデータが返ってくる

    def get_full(self, match):
        return "http://i.imgur.com/%s.jpg" % match.group(1)

    def get_full_https(self, match):
        return None

    def get_large(self, match):
        return "http://i.imgur.com/%sl.jpg" % match.group(1)

    def get_large_https(self, match):
        return None

    def get_thumb(self, match):
        return "http://i.imgur.com/%ss.jpg" % match.group(1)

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
