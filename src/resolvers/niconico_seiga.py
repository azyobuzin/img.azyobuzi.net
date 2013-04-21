# -*- coding: utf-8 -*-

from resolvers import *

class NiconicoSeiga(Resolver):
    @property
    def service_name(self):
        return u"ニコニコ静画"

    @property
    def regex_str(self):
        return r"^https?://(?:seiga\.nicovideo\.jp/seiga|nico\.ms)/im(\d+)(?:\?.*)?$"

    def get_full(self, match):
        return "http://seiga.nicovideo.jp/image/source?id=" + match.group(1)

    def get_full_https(self, match):
        return None

    def get_large(self, match):
        return "http://lohas.nicoseiga.jp/thumb/%si" % match.group(1)

    def get_large_https(self, match):
        return None

    def get_thumb(self, match):
        return "http://lohas.nicoseiga.jp/thumb/" + match.group(1)

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
