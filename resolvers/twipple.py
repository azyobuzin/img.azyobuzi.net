# -*- coding: utf-8 -*-

import re

class twipple:
	def __str__(self):
		return u"ついっぷるフォト"
	
	regexStr = "^http://(?:p.twipple\\.jp|(?:p.twipple\\.jp|p.twpl.jp)/show/\\w+)/(\\w+)(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://p.twpl.jp/show/orig/" + match.group(1)
	
	def getLargeSize(self, match):
		return "http://p.twipple.jp/show/large/" + match.group(1)
	
	def getThumbnail(self, match):
		return "http://p.twipple.jp/show/thumb/" + match.group(1)
