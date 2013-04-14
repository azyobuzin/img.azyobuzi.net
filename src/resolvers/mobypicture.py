# -*- coding: utf-8 -*-

from resolvers import *

class Mobypicture(Resolver): #メモ：twitpic 同様 http にリダイレクトされる
    @property
    def service_name(self):
        return "Mobypicture"

    @property
    def regex_str(self):
        return r"^http://(?:www\.)?moby\.to/(\w+)(?:\:\w*)?(?:\?.*)?$"

    def get_full(self, match):
        return "http://moby.to/%s:full" % match.group(1)

    def get_full_https(self, match):
        return "https://moby.to/%s:full" % match.group(1)

    def get_large(self, match):
        return "http://moby.to/%s:medium" % match.group(1)

    def get_large_https(self, match):
        return "https://moby.to/%s:medium" % match.group(1)

    def get_thumb(self, match):
        return "http://moby.to/%s:thumb" % match.group(1)

    def get_thumb_https(self, match):
        return "https://moby.to/%s:thumb" % match.group(1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
