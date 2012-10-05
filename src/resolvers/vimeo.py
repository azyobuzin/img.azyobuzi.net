# -*- coding: utf-8 -*-

import re
import json
import urllib2
import MySQLdb
from private_constant import *

class vimeo:
	def __str__(self):
		return "Vimeo"
	
	regexStr = "^https?://(?:www\\.)?vimeo\\.com/(\\d+)/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		id = match.group(1)
		
		if self.last is not None and str(self.last[0]) == id:
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM vimeo WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			httpRes = None
			
			try:
				httpRes = urllib2.urlopen("http://vimeo.com/api/v2/video/%s.json" % id)
			except:
				return None
			
			j = json.loads(httpRes.read())[0]
			
			small = j["thumbnail_small"]
			large = j["thumbnail_large"]
			
			c.execute("INSERT INTO vimeo VALUES (%s, %s, %s)"
				% (db.literal(id), db.literal(small), db.literal(large))
			)
			
			db.commit()
			
			self.last = [id, small, large]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		return data[2] if data is not None else None
	
	def getLargeSize(self, match):
		data = self.getUriData(match)
		return data[2] if data is not None else None
	
	def getThumbnail(self, match):
		data = self.getUriData(match)
		return data[1] if data is not None else None
