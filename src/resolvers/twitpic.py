# -*- coding: utf-8 -*-

##import re
##
##class twitpic:
##	def __str__(self):
##		return "Twitpic"
##
##	regexStr = "^https?://(?:www\\.)?twitpic\\.com/(?:show/\\w+/)?(\\w+)/?(?:\\?.*)?$"
##	regex = re.compile(regexStr, re.IGNORECASE)
##
##	def getFullSize(self, match):
##		return "http://twitpic.com/show/full/" + match.group(1)
##
##	def getLargeSize(self, match):
##		return "http://twitpic.com/show/large/" + match.group(1)
##
##	def getThumbnail(self, match):
##		return "http://twitpic.com/show/thumb/" + match.group(1)

from resolvers import Resolver

class Twitpic(Resolver): #メモ：スキーマを https にしても結局 http にリダイレクトされる
    @property
    def service_name(self):
        return "Twitpic"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?twitpic\.com/(?:show/\w+/)?(\w+)/?(?:\?.*)?$"

    def get_full(self, match):
        return "http://twitpic.com/show/full/" + match.group(1)

    def get_full_https(self, match):
        return "https://twitpic.com/show/full/" + match.group(1)

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
