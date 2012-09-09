# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
import xml.etree.ElementTree as ET
from private_constant import *

class niconico:
	def __str__(self):
		return u"niconico（ニコニコ動画）"
	
	regexStr = "^http://(?:(?:www\.)?nicovideo\\.jp/watch|nico\\.ms)/([sn]m\\d+)?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		id = match.group(1)
		
		if self.last is not None and str(self.last[0]) == str(id):
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM niconico WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			httpRes = urllib2.urlopen("http://ext.nicovideo.jp/api/getthumbinfo/" + id)
			
			root = ET.fromstring(httpRes.read())
			thumb = root.find("thumb")
			
			if thumb is None:
				return None
			
			thumbnail = thumb.find("thumbnail_url").text
			
			c.execute("INSERT INTO niconico VALUES (%s, %s)"
				% (db.literal(id), db.literal(thumbnail))
			)
			
			db.commit()
			
			self.last = [id, thumbnail]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		if data is not None:
			return data[1] + (".L" if int(data[0][2:]) >= 16371850 else "")
		else:
			return None
	
	def getLargeSize(self, match):
		data = self.getUriData(match)
		if data is not None:
			return data[1] + (".L" if int(data[0][2:]) >= 16371850 else "")
		else:
			return None
	
	def getThumbnail(self, match):
		data = self.getUriData(match)
		return data[1] if data is not None else None
