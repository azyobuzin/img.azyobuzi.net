# -*- coding: utf-8 -*-

import re

class niconicoSeiga:
	regexStr = "^https?://(?:seiga\\.nicovideo\\.jp/seiga|nico\\.ms)/im(\\d+)(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://seiga.nicovideo.jp/image/source?id=" + match.group(1)
	
	def getLargeSize(self, match):
		return "http://lohas.nicoseiga.jp/thumb/" + match.group(1) + "i"
	
	def getThumbnail(self, match):
		return "http://lohas.nicoseiga.jp/thumb/" + match.group(1)
