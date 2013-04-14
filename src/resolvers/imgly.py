# -*- coding: utf-8 -*-

from resolvers import *

class Imgly(Resolver): #メモ：twitpic 同様 http にリダイレクトされる
    @property
    def service_name(self):
        return "img.ly"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?img\.ly/(?:show/\w+/)?(\w+)/?(?:\?.*)?$"

    def get_full(self, match):
        return "http://img.ly/show/full/" + match.group(1)

    def get_full_https(self, match):
        return "https://img.ly/show/full/" + match.group(1)

    def get_large(self, match):
        return "http://img.ly/show/large/" + match.group(1)

    def get_large_https(self, match):
        return "https://img.ly/show/large/" + match.group(1)

    def get_thumb(self, match):
        return "http://img.ly/show/thumb/" + match.group(1)

    def get_thumb_https(self, match):
        return "https://img.ly/show/thumb/" + match.group(1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
