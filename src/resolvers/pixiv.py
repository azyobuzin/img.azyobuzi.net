# -*- coding: utf-8 -*-

from resolvers import *

class Pixiv(Resolver):
    @property
    def service_name(self):
        return "pixiv"

    @property
    def regex_str(self):
       return r"^http://(?:www\.)?pixiv\.net/(?:index|member_illust)\.php\?(?:.*)&?illust_id=(\d+)(?:&.*)?$"

    def get_full(self, match):
        return "http://img.azyobuzi.net/api/pixiv_proxy.cgi?size=full&id=" + match.group(1)

    def get_full_https(self, match):
        return None #誰か SSL サーバ証明書買って…

    def get_large(self, match):
        return "http://img.azyobuzi.net/api/pixiv_proxy.cgi?size=large&id=" + match.group(1)

    def get_large_https(self, match):
        return None

    def get_thumb(self, match):
        return "http://img.azyobuzi.net/api/pixiv_proxy.cgi?size=thumb&id=" + match.group(1)

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
