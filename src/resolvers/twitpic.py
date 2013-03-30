# -*- coding: utf-8 -*-

from resolvers import *

class Twitpic(Resolver): #メモ：スキーマを https にしても結局 http にリダイレクトされる
    @property
    def service_name(self):
        return "Twitpic"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?twitpic\.com/(?:show/\w+/)?(\w+)/?(?:\?.*)?$"

    def get_full(self, match):
        return self.get_large(match)

    def get_full_https(self, match):
        return self.get_large_https(match)

    def get_large(self, match):
        return "http://twitpic.com/show/large/" + match.group(1)

    def get_large_https(self, match):
        return "https://twitpic.com/show/large/" + match.group(1)

    def get_thumb(self, match):
        return "http://twitpic.com/show/thumb/" + match.group(1)

    def get_thumb_https(self, match):
        return "https://twitpic.com/show/thumb/" + match.group(1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
