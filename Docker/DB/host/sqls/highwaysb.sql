--
-- PostgreSQL database dump
--

-- Dumped from database version 9.6.6
-- Dumped by pg_dump version 9.6.5

-- Started on 2018-01-26 13:01:49

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 1 (class 3079 OID 12989)
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- TOC entry 3196 (class 0 OID 0)
-- Dependencies: 1
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


SET search_path = public, pg_catalog;

--
-- TOC entry 235 (class 1255 OID 16386)
-- Name: create_index(); Type: FUNCTION; Schema: public; Owner: flexberryhwsbuser
--

CREATE FUNCTION create_index() RETURNS void
    LANGUAGE plpgsql
    AS $$
BEGIN
   IF NOT EXISTS (SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
                  WHERE CONSTRAINT_NAME = 'STORMI_FSTORMAG_0') THEN
	ALTER TABLE STORMI ADD CONSTRAINT STORMI_FSTORMAG_0 FOREIGN KEY (User_m0) REFERENCES STORMAG;
	CREATE INDEX STORMI_IUser_m0 on STORMI (User_m0);
   END IF;
END;
$$;


ALTER FUNCTION public.create_index() OWNER TO flexberryhwsbuser;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 231 (class 1259 OID 16813)
-- Name: applicationlog; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE applicationlog (
    primarykey uuid NOT NULL,
    category character varying(64),
    eventid integer,
    priority integer,
    severity character varying(32),
    title character varying(256),
    "timestamp" timestamp(3) without time zone,
    machinename character varying(32),
    appdomainname character varying(512),
    processid character varying(256),
    processname character varying(512),
    threadname character varying(512),
    win32threadid character varying(128),
    message character varying(2500),
    formattedmessage text
);


ALTER TABLE applicationlog OWNER TO flexberryhwsbuser;

--
-- TOC entry 221 (class 1259 OID 16736)
-- Name: compressionsetting; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE compressionsetting (
    primarykey uuid NOT NULL,
    lifetimeunits character varying(6),
    lifetimelimit integer,
    targetcompression character varying(13),
    periodunits character varying(6),
    period integer,
    nextcompressiontime timestamp(3) without time zone,
    lastcompressiontime timestamp(3) without time zone,
    statsetting uuid NOT NULL
);


ALTER TABLE compressionsetting OWNER TO flexberryhwsbuser;

--
-- TOC entry 201 (class 1259 OID 16597)
-- Name: event; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE event (
    primarykey uuid NOT NULL,
    "time" timestamp(3) without time zone,
    status character varying(15),
    description character varying(255),
    exception character varying,
    request character varying,
    response character varying,
    isreported boolean,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255),
    watcher uuid NOT NULL
);


ALTER TABLE event OWNER TO flexberryhwsbuser;

--
-- TOC entry 203 (class 1259 OID 16613)
-- Name: exceptionsset; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE exceptionsset (
    primarykey uuid NOT NULL,
    name character varying NOT NULL
);


ALTER TABLE exceptionsset OWNER TO flexberryhwsbuser;

--
-- TOC entry 206 (class 1259 OID 16634)
-- Name: informer; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE informer (
    primarykey uuid NOT NULL,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255),
    repeatinterval integer,
    timereported timestamp(3) without time zone,
    name character varying(255),
    email character varying(255),
    reporttopictemplate character varying,
    reporttemplate character varying,
    isactive boolean,
    reporttype character varying(4),
    repeat boolean,
    sendsms boolean,
    phonenumbers character varying,
    smstemplate character varying
);


ALTER TABLE informer OWNER TO flexberryhwsbuser;

--
-- TOC entry 216 (class 1259 OID 16702)
-- Name: logmsg; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE logmsg (
    primarykey uuid NOT NULL,
    msgid uuid NOT NULL
);


ALTER TABLE logmsg OWNER TO flexberryhwsbuser;

--
-- TOC entry 212 (class 1259 OID 16676)
-- Name: outboundmessagetyperestriction; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE outboundmessagetyperestriction (
    primarykey uuid NOT NULL,
    "ТипСообщения" uuid NOT NULL,
    "Клиент" uuid NOT NULL
);


ALTER TABLE outboundmessagetyperestriction OWNER TO flexberryhwsbuser;

--
-- TOC entry 199 (class 1259 OID 16581)
-- Name: scheme; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE scheme (
    primarykey uuid NOT NULL,
    name character varying(255),
    availableforreading boolean,
    availableforediting boolean,
    creator character varying(255)
);


ALTER TABLE scheme OWNER TO flexberryhwsbuser;

--
-- TOC entry 198 (class 1259 OID 16576)
-- Name: schemeitem; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE schemeitem (
    primarykey uuid NOT NULL,
    name character varying(255),
    itemtype character varying(13),
    posx integer,
    posy integer,
    watcher uuid,
    groupscheme uuid,
    "Клиент" uuid,
    scheme uuid NOT NULL
);


ALTER TABLE schemeitem OWNER TO flexberryhwsbuser;

--
-- TOC entry 197 (class 1259 OID 16571)
-- Name: schemeitemlink; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE schemeitemlink (
    primarykey uuid NOT NULL,
    name character varying(255),
    direction character varying(19),
    target uuid NOT NULL,
    source uuid NOT NULL,
    scheme uuid NOT NULL
);


ALTER TABLE schemeitemlink OWNER TO flexberryhwsbuser;

--
-- TOC entry 185 (class 1259 OID 16387)
-- Name: session; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE session (
    primarykey uuid NOT NULL,
    userkey uuid,
    startedat timestamp(3) without time zone,
    lastaccess timestamp(3) without time zone,
    closed boolean
);


ALTER TABLE session OWNER TO flexberryhwsbuser;

--
-- TOC entry 202 (class 1259 OID 16605)
-- Name: statisticsmonitor; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE statisticsmonitor (
    primarykey uuid NOT NULL,
    "Наименование" character varying(255) NOT NULL,
    "ДоступенДругимПользователям" boolean,
    "Логин" character varying(255),
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE statisticsmonitor OWNER TO flexberryhwsbuser;

--
-- TOC entry 213 (class 1259 OID 16681)
-- Name: statrecord; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE statrecord (
    primarykey uuid NOT NULL,
    since timestamp(3) without time zone,
    "To" timestamp(3) without time zone,
    statinterval character varying(13),
    sentcount integer,
    receivedcount integer,
    errorscount integer,
    uniqueerrorscount integer,
    queuelength integer,
    avgtimesent integer,
    avgtimesql integer,
    connectioncount integer,
    sumtimesent integer,
    counttimesent integer,
    sumtimesql integer,
    counttimesql integer,
    statsetting uuid NOT NULL
);


ALTER TABLE statrecord OWNER TO flexberryhwsbuser;

--
-- TOC entry 217 (class 1259 OID 16707)
-- Name: statsetting; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE statsetting (
    primarykey uuid NOT NULL,
    "Подписка" uuid
);


ALTER TABLE statsetting OWNER TO flexberryhwsbuser;

--
-- TOC entry 186 (class 1259 OID 16390)
-- Name: stormac; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormac (
    primarykey uuid NOT NULL,
    typeaccess character varying(7),
    filter_m0 uuid,
    permition_m0 uuid NOT NULL,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE stormac OWNER TO flexberryhwsbuser;

--
-- TOC entry 225 (class 1259 OID 16765)
-- Name: stormadvlimit; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormadvlimit (
    primarykey uuid NOT NULL,
    "User" character varying(255),
    published boolean,
    module character varying(255),
    name character varying(255),
    value text,
    hotkeydata integer
);


ALTER TABLE stormadvlimit OWNER TO flexberryhwsbuser;

--
-- TOC entry 187 (class 1259 OID 16396)
-- Name: stormag; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormag (
    primarykey uuid NOT NULL,
    name character varying(80) NOT NULL,
    login character varying(50),
    pwd character varying(50),
    isuser boolean NOT NULL,
    isgroup boolean NOT NULL,
    isrole boolean NOT NULL,
    connstring character varying(255),
    enabled boolean,
    email character varying(80),
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE stormag OWNER TO flexberryhwsbuser;

--
-- TOC entry 233 (class 1259 OID 16826)
-- Name: stormauentity; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormauentity (
    primarykey uuid NOT NULL,
    objectprimarykey character varying(38) NOT NULL,
    operationtime timestamp(3) without time zone NOT NULL,
    operationtype character varying(100) NOT NULL,
    executionresult character varying(12) NOT NULL,
    source character varying(255) NOT NULL,
    serializedfield text,
    user_m0 uuid NOT NULL,
    objecttype_m0 uuid NOT NULL
);


ALTER TABLE stormauentity OWNER TO flexberryhwsbuser;

--
-- TOC entry 234 (class 1259 OID 16834)
-- Name: stormaufield; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormaufield (
    primarykey uuid NOT NULL,
    field character varying(100) NOT NULL,
    oldvalue text,
    newvalue text,
    mainchange_m0 uuid,
    auditentity_m0 uuid NOT NULL
);


ALTER TABLE stormaufield OWNER TO flexberryhwsbuser;

--
-- TOC entry 232 (class 1259 OID 16821)
-- Name: stormauobjtype; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormauobjtype (
    primarykey uuid NOT NULL,
    name character varying(255) NOT NULL
);


ALTER TABLE stormauobjtype OWNER TO flexberryhwsbuser;

--
-- TOC entry 188 (class 1259 OID 16402)
-- Name: stormf; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormf (
    primarykey uuid NOT NULL,
    filtertext text,
    name character varying(255),
    filtertypenview character varying(255),
    subject_m0 uuid,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE stormf OWNER TO flexberryhwsbuser;

--
-- TOC entry 228 (class 1259 OID 16789)
-- Name: stormfilterdetail; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormfilterdetail (
    primarykey uuid NOT NULL,
    caption character varying(255) NOT NULL,
    dataobjectview character varying(255) NOT NULL,
    connectmasterprop character varying(255) NOT NULL,
    ownerconnectprop character varying(255),
    filtersetting_m0 uuid NOT NULL
);


ALTER TABLE stormfilterdetail OWNER TO flexberryhwsbuser;

--
-- TOC entry 229 (class 1259 OID 16797)
-- Name: stormfilterlookup; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormfilterlookup (
    primarykey uuid NOT NULL,
    dataobjecttype character varying(255) NOT NULL,
    container character varying(255),
    containertag character varying(255),
    fieldstoview character varying(255),
    filtersetting_m0 uuid NOT NULL
);


ALTER TABLE stormfilterlookup OWNER TO flexberryhwsbuser;

--
-- TOC entry 226 (class 1259 OID 16773)
-- Name: stormfiltersetting; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormfiltersetting (
    primarykey uuid NOT NULL,
    name character varying(255) NOT NULL,
    dataobjectview character varying(255) NOT NULL
);


ALTER TABLE stormfiltersetting OWNER TO flexberryhwsbuser;

--
-- TOC entry 189 (class 1259 OID 16408)
-- Name: stormi; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormi (
    primarykey uuid NOT NULL,
    user_m0 uuid NOT NULL,
    agent_m0 uuid NOT NULL,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE stormi OWNER TO flexberryhwsbuser;

--
-- TOC entry 190 (class 1259 OID 16414)
-- Name: stormla; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormla (
    primarykey uuid NOT NULL,
    view_m0 uuid NOT NULL,
    attribute_m0 uuid NOT NULL,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE stormla OWNER TO flexberryhwsbuser;

--
-- TOC entry 191 (class 1259 OID 16420)
-- Name: stormlg; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormlg (
    primarykey uuid NOT NULL,
    group_m0 uuid NOT NULL,
    user_m0 uuid NOT NULL,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE stormlg OWNER TO flexberryhwsbuser;

--
-- TOC entry 192 (class 1259 OID 16426)
-- Name: stormlo; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormlo (
    primarykey uuid NOT NULL,
    class_m0 uuid NOT NULL,
    operation_m0 uuid NOT NULL,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE stormlo OWNER TO flexberryhwsbuser;

--
-- TOC entry 193 (class 1259 OID 16432)
-- Name: stormlr; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormlr (
    primarykey uuid NOT NULL,
    startdate timestamp(3) without time zone,
    enddate timestamp(3) without time zone,
    agent_m0 uuid NOT NULL,
    role_m0 uuid NOT NULL,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE stormlr OWNER TO flexberryhwsbuser;

--
-- TOC entry 194 (class 1259 OID 16438)
-- Name: stormlv; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormlv (
    primarykey uuid NOT NULL,
    class_m0 uuid NOT NULL,
    view_m0 uuid NOT NULL,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE stormlv OWNER TO flexberryhwsbuser;

--
-- TOC entry 223 (class 1259 OID 16749)
-- Name: stormnetlockdata; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormnetlockdata (
    lockkey character varying(300) NOT NULL,
    username character varying(300) NOT NULL,
    lockdate timestamp(3) without time zone
);


ALTER TABLE stormnetlockdata OWNER TO flexberryhwsbuser;

--
-- TOC entry 195 (class 1259 OID 16444)
-- Name: stormp; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormp (
    primarykey uuid NOT NULL,
    subject_m0 uuid NOT NULL,
    agent_m0 uuid NOT NULL,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE stormp OWNER TO flexberryhwsbuser;

--
-- TOC entry 196 (class 1259 OID 16450)
-- Name: storms; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE storms (
    primarykey uuid NOT NULL,
    name character varying(100) NOT NULL,
    type character varying(100),
    isattribute boolean NOT NULL,
    isoperation boolean NOT NULL,
    isview boolean NOT NULL,
    isclass boolean NOT NULL,
    sharedoper boolean,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE storms OWNER TO flexberryhwsbuser;

--
-- TOC entry 224 (class 1259 OID 16757)
-- Name: stormsettings; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormsettings (
    primarykey uuid NOT NULL,
    module character varying(1000),
    name character varying(255),
    value text,
    "User" character varying(255)
);


ALTER TABLE stormsettings OWNER TO flexberryhwsbuser;

--
-- TOC entry 227 (class 1259 OID 16781)
-- Name: stormwebsearch; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE stormwebsearch (
    primarykey uuid NOT NULL,
    name character varying(255) NOT NULL,
    "Order" integer NOT NULL,
    presentview character varying(255) NOT NULL,
    detailedview character varying(255) NOT NULL,
    filtersetting_m0 uuid NOT NULL
);


ALTER TABLE stormwebsearch OWNER TO flexberryhwsbuser;

--
-- TOC entry 209 (class 1259 OID 16655)
-- Name: substatisticsmonitor; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE substatisticsmonitor (
    primarykey uuid NOT NULL,
    "Категория" character varying(255),
    "Код" integer,
    "Наименование" character varying(255),
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255),
    "Подписка" uuid NOT NULL,
    statisticsmonitor uuid NOT NULL
);


ALTER TABLE substatisticsmonitor OWNER TO flexberryhwsbuser;

--
-- TOC entry 230 (class 1259 OID 16805)
-- Name: usersetting; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE usersetting (
    primarykey uuid NOT NULL,
    appname character varying(256),
    username character varying(512),
    userguid uuid,
    modulename character varying(1024),
    moduleguid uuid,
    settname character varying(256),
    settguid uuid,
    settlastaccesstime timestamp(3) without time zone,
    strval character varying(256),
    txtval text,
    intval integer,
    boolval boolean,
    guidval uuid,
    decimalval numeric(20,10),
    datetimeval timestamp(3) without time zone
);


ALTER TABLE usersetting OWNER TO flexberryhwsbuser;

--
-- TOC entry 207 (class 1259 OID 16642)
-- Name: watcher; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE watcher (
    primarykey uuid NOT NULL,
    category character varying(255),
    name character varying(255),
    comment character varying(255),
    isactive boolean,
    "interval" integer,
    address character varying(255),
    requesttemplate character varying,
    responsetemplate character varying,
    connectionstring character varying(255),
    type character varying(18),
    soapaction character varying(255),
    systemid character varying(255),
    timetorespond timestamp(3) without time zone,
    usetriplecheck boolean,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255),
    timeoflastcheck timestamp(3) without time zone
);


ALTER TABLE watcher OWNER TO flexberryhwsbuser;

--
-- TOC entry 211 (class 1259 OID 16671)
-- Name: watcherexceptionsset; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE watcherexceptionsset (
    primarykey uuid NOT NULL,
    active boolean,
    watcher_m0 uuid NOT NULL,
    exceptionsset_m0 uuid NOT NULL
);


ALTER TABLE watcherexceptionsset OWNER TO flexberryhwsbuser;

--
-- TOC entry 208 (class 1259 OID 16650)
-- Name: watchergroupitem; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE watchergroupitem (
    primarykey uuid NOT NULL,
    nestedwatcher uuid NOT NULL,
    watcher uuid NOT NULL
);


ALTER TABLE watchergroupitem OWNER TO flexberryhwsbuser;

--
-- TOC entry 200 (class 1259 OID 16589)
-- Name: watcherinformer; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE watcherinformer (
    primarykey uuid NOT NULL,
    isactive boolean,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255),
    watcher uuid NOT NULL,
    informer uuid NOT NULL
);


ALTER TABLE watcherinformer OWNER TO flexberryhwsbuser;

--
-- TOC entry 205 (class 1259 OID 16629)
-- Name: watchexception; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE watchexception (
    primarykey uuid NOT NULL,
    name character varying(100),
    active boolean,
    period character varying(20),
    months character varying(100),
    days character varying(100),
    start time without time zone,
    durationhr double precision,
    exceptionsset_m0 uuid NOT NULL
);


ALTER TABLE watchexception OWNER TO flexberryhwsbuser;

--
-- TOC entry 219 (class 1259 OID 16720)
-- Name: Клиент; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE "Клиент" (
    primarykey uuid NOT NULL,
    "Наименование" character varying(64),
    "Ид" character varying(255),
    "Адрес" character varying(255),
    dnsidentity character varying(255),
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE "Клиент" OWNER TO flexberryhwsbuser;

--
-- TOC entry 210 (class 1259 OID 16663)
-- Name: Монитор; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE "Монитор" (
    primarykey uuid NOT NULL,
    "Наименование" character varying(255) NOT NULL,
    "ДоступенДругимПользователям" boolean,
    "Логин" character varying(255),
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE "Монитор" OWNER TO flexberryhwsbuser;

--
-- TOC entry 214 (class 1259 OID 16686)
-- Name: Подписка; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE "Подписка" (
    primarykey uuid NOT NULL,
    "ДатаПрекращения" bigint NOT NULL,
    iscallback boolean,
    "НеудПопытки" integer,
    "ПередаватьПо" character varying(4),
    "Описание" character varying(255),
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255),
    "ТипСообщения_m0" uuid NOT NULL,
    "Клиент_m0" uuid,
    "Клиент_m1" uuid
);


ALTER TABLE "Подписка" OWNER TO flexberryhwsbuser;

--
-- TOC entry 204 (class 1259 OID 16621)
-- Name: ПодпискаМонитора; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE "ПодпискаМонитора" (
    primarykey uuid NOT NULL,
    "Категория" character varying(255) NOT NULL,
    "Код" integer,
    "Наименование" character varying(255) NOT NULL,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255),
    "Подписка" uuid NOT NULL,
    "Монитор" uuid NOT NULL
);


ALTER TABLE "ПодпискаМонитора" OWNER TO flexberryhwsbuser;

--
-- TOC entry 222 (class 1259 OID 16741)
-- Name: Сообщение; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE "Сообщение" (
    primarykey uuid NOT NULL,
    "ВремяСледующейОтправки" timestamp(3) without time zone NOT NULL,
    "Тело" character varying,
    "ВремяФормирования" timestamp(3) without time zone NOT NULL,
    "Отправитель" character varying(255),
    "ВложениеДляБазы" character varying,
    "Приоритет" integer,
    "ИмяГруппы" character varying(255),
    "Отправляется" boolean,
    failscount integer,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255),
    "Тэги" character varying,
    logmessages character varying,
    "ТипСообщения_m0" uuid NOT NULL,
    "Получатель_m0" uuid,
    "Получатель_m1" uuid
);


ALTER TABLE "Сообщение" OWNER TO flexberryhwsbuser;

--
-- TOC entry 218 (class 1259 OID 16712)
-- Name: ТипСообщения; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE "ТипСообщения" (
    primarykey uuid NOT NULL,
    "Наименование" character varying(64),
    "Ид" character varying(255),
    "Комментарий" character varying,
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255)
);


ALTER TABLE "ТипСообщения" OWNER TO flexberryhwsbuser;

--
-- TOC entry 220 (class 1259 OID 16728)
-- Name: Тэг; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE "Тэг" (
    primarykey uuid NOT NULL,
    "Имя" character varying(255),
    "Значение" character varying(255),
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255),
    "Сообщение_m0" uuid NOT NULL
);


ALTER TABLE "Тэг" OWNER TO flexberryhwsbuser;

--
-- TOC entry 215 (class 1259 OID 16694)
-- Name: Шина; Type: TABLE; Schema: public; Owner: flexberryhwsbuser
--

CREATE TABLE "Шина" (
    primarykey uuid NOT NULL,
    "interopАдрес" character varying(255),
    createtime timestamp(3) without time zone,
    creator character varying(255),
    edittime timestamp(3) without time zone,
    editor character varying(255),
    "Наименование" character varying(64),
    "Ид" character varying(255),
    "Адрес" character varying(255),
    dnsidentity character varying(255)
);


ALTER TABLE "Шина" OWNER TO flexberryhwsbuser;

--
-- TOC entry 3186 (class 0 OID 16813)
-- Dependencies: 231
-- Data for Name: applicationlog; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY applicationlog (primarykey, category, eventid, priority, severity, title, "timestamp", machinename, appdomainname, processid, processname, threadname, win32threadid, message, formattedmessage) FROM stdin;
\.


--
-- TOC entry 3176 (class 0 OID 16736)
-- Dependencies: 221
-- Data for Name: compressionsetting; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY compressionsetting (primarykey, lifetimeunits, lifetimelimit, targetcompression, periodunits, period, nextcompressiontime, lastcompressiontime, statsetting) FROM stdin;
\.


--
-- TOC entry 3156 (class 0 OID 16597)
-- Dependencies: 201
-- Data for Name: event; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY event (primarykey, "time", status, description, exception, request, response, isreported, createtime, creator, edittime, editor, watcher) FROM stdin;
\.


--
-- TOC entry 3158 (class 0 OID 16613)
-- Dependencies: 203
-- Data for Name: exceptionsset; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY exceptionsset (primarykey, name) FROM stdin;
\.


--
-- TOC entry 3161 (class 0 OID 16634)
-- Dependencies: 206
-- Data for Name: informer; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY informer (primarykey, createtime, creator, edittime, editor, repeatinterval, timereported, name, email, reporttopictemplate, reporttemplate, isactive, reporttype, repeat, sendsms, phonenumbers, smstemplate) FROM stdin;
\.


--
-- TOC entry 3171 (class 0 OID 16702)
-- Dependencies: 216
-- Data for Name: logmsg; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY logmsg (primarykey, msgid) FROM stdin;
\.


--
-- TOC entry 3167 (class 0 OID 16676)
-- Dependencies: 212
-- Data for Name: outboundmessagetyperestriction; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY outboundmessagetyperestriction (primarykey, "ТипСообщения", "Клиент") FROM stdin;
\.


--
-- TOC entry 3154 (class 0 OID 16581)
-- Dependencies: 199
-- Data for Name: scheme; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY scheme (primarykey, name, availableforreading, availableforediting, creator) FROM stdin;
\.


--
-- TOC entry 3153 (class 0 OID 16576)
-- Dependencies: 198
-- Data for Name: schemeitem; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY schemeitem (primarykey, name, itemtype, posx, posy, watcher, groupscheme, "Клиент", scheme) FROM stdin;
\.


--
-- TOC entry 3152 (class 0 OID 16571)
-- Dependencies: 197
-- Data for Name: schemeitemlink; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY schemeitemlink (primarykey, name, direction, target, source, scheme) FROM stdin;
\.


--
-- TOC entry 3140 (class 0 OID 16387)
-- Dependencies: 185
-- Data for Name: session; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY session (primarykey, userkey, startedat, lastaccess, closed) FROM stdin;
\.


--
-- TOC entry 3157 (class 0 OID 16605)
-- Dependencies: 202
-- Data for Name: statisticsmonitor; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY statisticsmonitor (primarykey, "Наименование", "ДоступенДругимПользователям", "Логин", createtime, creator, edittime, editor) FROM stdin;
\.


--
-- TOC entry 3168 (class 0 OID 16681)
-- Dependencies: 213
-- Data for Name: statrecord; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY statrecord (primarykey, since, "To", statinterval, sentcount, receivedcount, errorscount, uniqueerrorscount, queuelength, avgtimesent, avgtimesql, connectioncount, sumtimesent, counttimesent, sumtimesql, counttimesql, statsetting) FROM stdin;
\.


--
-- TOC entry 3172 (class 0 OID 16707)
-- Dependencies: 217
-- Data for Name: statsetting; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY statsetting (primarykey, "Подписка") FROM stdin;
\.


--
-- TOC entry 3141 (class 0 OID 16390)
-- Dependencies: 186
-- Data for Name: stormac; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormac (primarykey, typeaccess, filter_m0, permition_m0, createtime, creator, edittime, editor) FROM stdin;
a55e47ad-a740-4dec-8b1c-1242d2791454	Read	bea91ee7-ab73-484f-9bf0-f3a476f55288	8a4f5b9b-12cd-4599-911e-497b6e6f3fc3	\N	\N	\N	\N
1ff002ea-0e28-40c8-95ae-290d94bc39bd	Full	86954df9-c193-4517-b81c-ae8b4dea7420	8a4f5b9b-12cd-4599-911e-497b6e6f3fc3	\N	\N	\N	\N
a3765e56-2836-41cc-a0aa-9b098a2e2c48	Full	\N	160e05bc-41e6-4e16-9135-16debc6515f6	2018-01-26 07:57:35.714	admin	\N	\N
842584b2-3cc0-4356-a9e9-d653845c8404	Full	\N	2057032d-f73b-4dcc-955b-bdbbb42174a9	2018-01-26 07:57:35.714	admin	\N	\N
fc3be7fb-2c2d-4dcb-bf44-1079d28bd0ef	Full	\N	83699e83-6d99-44e1-9023-fde55e00a5a8	2018-01-26 07:57:35.714	admin	\N	\N
67941ee4-8be3-4860-845c-8954c916dc24	Full	\N	910bafbc-6d23-4d40-8a11-6e8906351c78	2018-01-26 07:57:35.714	admin	\N	\N
a38f056c-9c5d-4e77-8941-602e176cf796	Full	\N	4520011a-23ac-4aa3-9ff9-71869aa9c5a4	2018-01-26 07:57:35.715	admin	\N	\N
3181b1c5-b8fb-4630-88d9-c776a3d1ce3f	Full	\N	d94545ac-c0f8-4671-b291-c784feeb56e0	2018-01-26 07:57:35.715	admin	\N	\N
1ac0ce6b-8701-4cdb-b6c4-ffd16baabe9f	Full	\N	ecde3609-3bc3-4298-b50a-9dd61978b649	2018-01-26 07:57:35.715	admin	\N	\N
14350655-1f81-477e-b7ad-1ee9da5b08d6	Full	\N	661f7ba3-3dea-4997-bb27-19e3553261d6	2018-01-26 07:57:35.715	admin	\N	\N
98879f07-29b7-4474-901b-05f6a018aec7	Full	\N	fcbed4cb-d102-4a17-9f12-6b7259b37e59	2018-01-26 07:57:35.716	admin	\N	\N
b555c81b-be06-4bc5-897a-b590e6722a22	Full	\N	1d099001-49f4-4c40-a078-54a59f63f2c9	2018-01-26 07:57:51.774	admin	\N	\N
d4e5a515-050d-4bec-981f-c98f308882ac	Full	\N	44e81883-e2d4-4f68-9be6-dfa5eb937545	2018-01-26 07:57:51.775	admin	\N	\N
53f595b5-8aea-4ccb-9309-18ff79048c93	Read	\N	44e81883-e2d4-4f68-9be6-dfa5eb937545	2018-01-26 07:57:51.775	admin	\N	\N
\.


--
-- TOC entry 3180 (class 0 OID 16765)
-- Dependencies: 225
-- Data for Name: stormadvlimit; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormadvlimit (primarykey, "User", published, module, name, value, hotkeydata) FROM stdin;
\.


--
-- TOC entry 3142 (class 0 OID 16396)
-- Dependencies: 187
-- Data for Name: stormag; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormag (primarykey, name, login, pwd, isuser, isgroup, isrole, connstring, enabled, email, createtime, creator, edittime, editor) FROM stdin;
7b9cee01-bef5-4cbb-88b0-77ab212ae881	Все пользователи	\N	\N	f	f	t	\N	t	\N	\N	\N	\N	\N
082345bb-ba23-445c-a8a1-10b8ab84748b	Administrator	admin	D033E22AE348AEB5660FC2140AEC35850C4DA997	t	f	f	\N	t	\N	\N	\N	\N	\N
590de25f-c7f0-4019-a53a-645f769b7b57	admin	\N	\N	f	f	t	\N	t	\N	\N	\N	\N	\N
afa956da-0144-48b7-8a20-5ed57b6f03db	tst	tst	99875401D16283B911C70B1DDBC25AC40836367F	t	f	f	\N	t	\N	2018-01-26 07:19:54.125	admin	\N	\N
\.


--
-- TOC entry 3188 (class 0 OID 16826)
-- Dependencies: 233
-- Data for Name: stormauentity; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormauentity (primarykey, objectprimarykey, operationtime, operationtype, executionresult, source, serializedfield, user_m0, objecttype_m0) FROM stdin;
65efa8d1-0c9f-4ced-8465-ae130be874af	{921ba732-657c-45e0-bf62-64c56d6830d1}	2018-01-26 07:19:54.617	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	24e78568-10c1-424b-be66-d0840151ad13
848c55a9-1777-4e14-b3ec-6340e9993225	{afa956da-0144-48b7-8a20-5ed57b6f03db}	2018-01-26 07:19:54.617	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	26cd08ef-1419-40d7-9396-1e51d9a6d8ab
08c094b9-3ee9-4d7f-80d4-1c02819409c2	{981c869c-dbde-4a11-bf66-779edcb58c96}	2018-01-26 07:31:27.009	Удаление	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
d05b2de9-68e7-4688-b8ab-2230ac87bc5e	{c2b34e61-07aa-4c2a-9f20-8287a1a32986}	2018-01-26 07:31:27.009	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
21a1eafc-88f5-4781-a7dc-86dd160c8e57	{92174bb8-df03-4904-a2a5-f7ecd2951e10}	2018-01-26 07:31:27.009	Удаление	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
4d7cac52-effc-4bca-a167-67ca35b53fdb	{e4afeb95-6f3a-4fb1-bbbd-91e33af0c223}	2018-01-26 07:31:27.009	Удаление	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
9a127ad5-81fa-4f52-9980-f20cb1efdb00	{ccdfc400-223f-4960-b11f-fb6f632636ce}	2018-01-26 07:31:27.009	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
3299a62f-4e12-4c73-8fb8-d406112160dc	{68b1d2c8-4fbb-4f94-91a8-2fbe18e5d2c8}	2018-01-26 07:31:27.009	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
d0c67a74-2c17-428f-ad25-3cc03eced9cc	{76c27031-bc9a-4103-a1ad-cd33987097a6}	2018-01-26 07:55:32.698	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	f33336ec-f5c6-40e5-81e0-54a273deef06
4eae13ea-b54a-48b3-b22d-2e77cb556e1e	{35963c75-7a21-49b4-9222-0e4dcd356a67}	2018-01-26 07:55:48.559	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	f33336ec-f5c6-40e5-81e0-54a273deef06
315f85fa-d1fe-4d61-b913-6da7a0f7e3f8	{6fa6221e-c2f9-4a94-b560-b23575410bc2}	2018-01-26 07:56:00.823	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	f33336ec-f5c6-40e5-81e0-54a273deef06
c71a7eca-ff85-4c87-8c15-cbd21042d963	{57a3968f-af04-4326-b72d-973bb7149e0f}	2018-01-26 07:56:12.516	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	f33336ec-f5c6-40e5-81e0-54a273deef06
9f584772-1bb7-44e5-be75-c1620355fa75	{c50c9d56-1a81-4b88-9c11-84559caa7f41}	2018-01-26 07:56:24.194	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	f33336ec-f5c6-40e5-81e0-54a273deef06
af944103-26a7-43e4-a0a0-b629f79c6e17	{9a39df06-e28e-4d44-a791-29cb705a0127}	2018-01-26 07:56:37.066	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	f33336ec-f5c6-40e5-81e0-54a273deef06
d1d36116-0c00-4818-ae61-3562acae906e	{37b65397-850e-445a-ad1b-ee34fa92eda2}	2018-01-26 07:56:48.337	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	f33336ec-f5c6-40e5-81e0-54a273deef06
3f98925f-10ee-45ff-91ca-baa6b00e1126	{f4347bc7-6e1a-447c-8f87-87d8f34329ee}	2018-01-26 07:57:00.905	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	f33336ec-f5c6-40e5-81e0-54a273deef06
eb04185e-6f45-4d55-9743-24eb7b1f68f2	{e495b619-89f6-4a03-8f11-8797ed477a3b}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
5bc0d754-4f14-4b02-b78d-9920b87862b3	{160e05bc-41e6-4e16-9135-16debc6515f6}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
4db0f195-b2eb-4108-886d-c9ae3f8692c9	{2057032d-f73b-4dcc-955b-bdbbb42174a9}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
db880dfc-609f-4ef1-b42b-767f0d0e554c	{83699e83-6d99-44e1-9023-fde55e00a5a8}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
64843b8d-b8aa-4e6f-8751-607e005be0c4	{910bafbc-6d23-4d40-8a11-6e8906351c78}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
de849d8f-3df6-4da0-9587-1a31f681a29b	{4520011a-23ac-4aa3-9ff9-71869aa9c5a4}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
a554cadb-2038-4b10-9186-da640c483e94	{d94545ac-c0f8-4671-b291-c784feeb56e0}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
3760548c-c390-46ab-8b63-281c86027da0	{ecde3609-3bc3-4298-b50a-9dd61978b649}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
82316f25-e5e8-49c0-b0fe-521a0715a480	{661f7ba3-3dea-4997-bb27-19e3553261d6}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
a260d821-74d0-4718-8fd6-199333ab483b	{fcbed4cb-d102-4a17-9f12-6b7259b37e59}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
d4a0b44b-df11-433c-8a62-a6f0e244830c	{2d3772cc-8eb3-4596-959c-dcc47eda189c}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
83a08ad8-5ab4-412a-96a5-49d1c2eab051	{a3765e56-2836-41cc-a0aa-9b098a2e2c48}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
685b57f7-2cad-4ea3-b3d5-1054c1997a61	{842584b2-3cc0-4356-a9e9-d653845c8404}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
91f675ad-97a5-4b17-b244-a708a777359e	{fc3be7fb-2c2d-4dcb-bf44-1079d28bd0ef}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
0a1e956c-86e6-431c-a3b9-780e73d2ab7e	{67941ee4-8be3-4860-845c-8954c916dc24}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
c105b2c6-6d9f-49c3-9035-19f48076eb9f	{a38f056c-9c5d-4e77-8941-602e176cf796}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
7be5d286-c862-45d6-94a2-f64d91874183	{3181b1c5-b8fb-4630-88d9-c776a3d1ce3f}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
e48ad3a9-e85f-4aad-adfb-1d5cb16ec9f6	{1ac0ce6b-8701-4cdb-b6c4-ffd16baabe9f}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
6152204e-228f-4bf1-bc8e-0cf84bb926ce	{14350655-1f81-477e-b7ad-1ee9da5b08d6}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
b64e602a-2df0-4650-980b-b92a4b247bde	{98879f07-29b7-4474-901b-05f6a018aec7}	2018-01-26 07:57:36.731	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
87c6977c-4928-4686-a450-b300818504f6	{c2b34e61-07aa-4c2a-9f20-8287a1a32986}	2018-01-26 07:57:52.443	Удаление	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
bd02141c-463a-4e0a-ad85-216eaba345d6	{e495b619-89f6-4a03-8f11-8797ed477a3b}	2018-01-26 07:57:52.443	Удаление	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
47294617-a308-487d-915b-fdae398109e4	{1d099001-49f4-4c40-a078-54a59f63f2c9}	2018-01-26 07:57:52.443	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
9ada43ef-6506-463c-96f4-086fef4b8be5	{44e81883-e2d4-4f68-9be6-dfa5eb937545}	2018-01-26 07:57:52.443	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	cd701c42-5686-4dae-b87d-806138bd8c57
af33161a-5efb-450a-82eb-3cb5e36b4b54	{ccdfc400-223f-4960-b11f-fb6f632636ce}	2018-01-26 07:57:52.443	Удаление	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
04db988e-ab00-452f-8d22-7e0299ebfdb7	{68b1d2c8-4fbb-4f94-91a8-2fbe18e5d2c8}	2018-01-26 07:57:52.443	Удаление	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
27997d66-2495-4425-9a50-f5b74d71566d	{2d3772cc-8eb3-4596-959c-dcc47eda189c}	2018-01-26 07:57:52.443	Удаление	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
aa316bc3-2bd7-427a-8ae6-2a24c9e5b171	{b555c81b-be06-4bc5-897a-b590e6722a22}	2018-01-26 07:57:52.443	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
0110d4df-0532-49df-b4b4-b98104517faf	{d4e5a515-050d-4bec-981f-c98f308882ac}	2018-01-26 07:57:52.443	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
629a10f0-3be0-4dc5-82b9-6a7263a7cd9d	{53f595b5-8aea-4ccb-9309-18ff79048c93}	2018-01-26 07:57:52.443	Создание	Выполнено	IP: 172.18.0.1; DNS: 172.18.0.1	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23
\.


--
-- TOC entry 3189 (class 0 OID 16834)
-- Dependencies: 234
-- Data for Name: stormaufield; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormaufield (primarykey, field, oldvalue, newvalue, mainchange_m0, auditentity_m0) FROM stdin;
38b1c343-2383-472c-8aef-b85165868755	StartDate	-NULL-	\N	\N	65efa8d1-0c9f-4ced-8465-ae130be874af
139f13a4-d336-462e-af0f-fcd6cb1f28e4	EndDate	-NULL-	\N	\N	65efa8d1-0c9f-4ced-8465-ae130be874af
92e35ce4-e541-4332-a433-32db56bbcbb1	Role	-NULL-	Agent(Имя роли=Все пользователи)	\N	65efa8d1-0c9f-4ced-8465-ae130be874af
34229ba2-203c-4cd1-91df-4420ce5e7150	LinkedPrimaryKey	-NULL-	{7b9cee01-bef5-4cbb-88b0-77ab212ae881}	92e35ce4-e541-4332-a433-32db56bbcbb1	65efa8d1-0c9f-4ced-8465-ae130be874af
2210d3c0-c5b9-4986-8111-f1319c787c8c	Agent	-NULL-	Agent(Имя агента=tst, Логин агента=tst)	\N	65efa8d1-0c9f-4ced-8465-ae130be874af
89351e93-52c8-4704-85f1-dc2d4d7d5457	LinkedPrimaryKey	-NULL-	{afa956da-0144-48b7-8a20-5ed57b6f03db}	2210d3c0-c5b9-4986-8111-f1319c787c8c	65efa8d1-0c9f-4ced-8465-ae130be874af
ee4005b1-8a3a-4d26-a4cb-ebbc89489e10	Name	-NULL-	tst	\N	848c55a9-1777-4e14-b3ec-6340e9993225
597d06b5-5505-482c-9597-d7caacdd3153	Login	-NULL-	tst	\N	848c55a9-1777-4e14-b3ec-6340e9993225
3794e90c-9bad-4295-9e40-e0c98a08c486	Pwd	-NULL-	99875401D16283B911C70B1DDBC25AC40836367F	\N	848c55a9-1777-4e14-b3ec-6340e9993225
fa42ae61-363e-48cd-8011-8998bd0c5193	IsUser	-NULL-	True	\N	848c55a9-1777-4e14-b3ec-6340e9993225
69f00154-7f73-48b7-9370-300280b7346e	IsGroup	-NULL-	False	\N	848c55a9-1777-4e14-b3ec-6340e9993225
65838e1e-17eb-4d3a-88d0-4095897ca139	IsRole	-NULL-	False	\N	848c55a9-1777-4e14-b3ec-6340e9993225
42494a3b-85a1-4394-84d3-3bf4dc0789c6	ConnString	-NULL-	\N	\N	848c55a9-1777-4e14-b3ec-6340e9993225
23166d56-01a0-4fbc-8f90-79dcb9bc58ed	Enabled	-NULL-	True	\N	848c55a9-1777-4e14-b3ec-6340e9993225
30237973-46db-46f0-b90f-d8fc9f1af36a	Email	-NULL-	\N	\N	848c55a9-1777-4e14-b3ec-6340e9993225
acf3b783-8743-40e5-ad33-0f72599b26d2	Agent	Agent(Имя агента=Все пользователи, Логин агента=null)	-NULL-	\N	08c094b9-3ee9-4d7f-80d4-1c02819409c2
87e523de-e741-453c-a76d-50dd138884e8	LinkedPrimaryKey	{7b9cee01-bef5-4cbb-88b0-77ab212ae881}	-NULL-	acf3b783-8743-40e5-ad33-0f72599b26d2	08c094b9-3ee9-4d7f-80d4-1c02819409c2
c9a5d51b-d449-457b-bb55-ee95c0a3dde7	Subject	Subject(Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	-NULL-	\N	08c094b9-3ee9-4d7f-80d4-1c02819409c2
5e9c34bd-11ad-4806-b554-d53364048843	LinkedPrimaryKey	{1d742d0b-e6fd-4b56-8363-61c5f2178c45}	-NULL-	c9a5d51b-d449-457b-bb55-ee95c0a3dde7	08c094b9-3ee9-4d7f-80d4-1c02819409c2
3a5c00fe-fd1e-48d3-893d-1a7e090ffc62	Access (0)	Access(Тип доступа=Read, Фильтр={feb7db2a-a2cc-42d3-a419-72e9da67ce5f}, Имя фильтра=Чтение для отчётов, Разрешение={981c869c-dbde-4a11-bf66-779edcb58c96}, Имя агента=Все пользователи, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	-NULL-	\N	08c094b9-3ee9-4d7f-80d4-1c02819409c2
9600c6a5-daf4-49cc-9fdd-79974cb15125	LinkedPrimaryKey	{92174bb8-df03-4904-a2a5-f7ecd2951e10}	-NULL-	3a5c00fe-fd1e-48d3-893d-1a7e090ffc62	08c094b9-3ee9-4d7f-80d4-1c02819409c2
26676f6e-1f97-47ee-9b6d-376bc9fe3a71	Access (1)	Access(Тип доступа=Full, Фильтр={60e50089-a9d4-42af-9045-8c6fc999bb80}, Имя фильтра=Полный доступ к отчётам, Разрешение={981c869c-dbde-4a11-bf66-779edcb58c96}, Имя агента=Все пользователи, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	-NULL-	\N	08c094b9-3ee9-4d7f-80d4-1c02819409c2
a6e8c482-7626-4915-ba91-819f9da4f272	LinkedPrimaryKey	{e4afeb95-6f3a-4fb1-bbbd-91e33af0c223}	-NULL-	26676f6e-1f97-47ee-9b6d-376bc9fe3a71	08c094b9-3ee9-4d7f-80d4-1c02819409c2
7cee2993-33e8-49c5-b950-39256376df87	Agent	-NULL-	Agent(Имя агента=null, Логин агента=null)	\N	d05b2de9-68e7-4688-b8ab-2230ac87bc5e
3d7acb4a-aa24-4b89-b9d4-2b748fd33d76	LinkedPrimaryKey	-NULL-	{7b9cee01-bef5-4cbb-88b0-77ab212ae881}	7cee2993-33e8-49c5-b950-39256376df87	d05b2de9-68e7-4688-b8ab-2230ac87bc5e
961f0742-a555-4429-bd54-5e8fc6c64356	Subject	-NULL-	Subject(Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	\N	d05b2de9-68e7-4688-b8ab-2230ac87bc5e
263c5b2e-68d6-4820-9aa1-38d5b9ea1235	LinkedPrimaryKey	-NULL-	{1d742d0b-e6fd-4b56-8363-61c5f2178c45}	961f0742-a555-4429-bd54-5e8fc6c64356	d05b2de9-68e7-4688-b8ab-2230ac87bc5e
dfe054d4-26ea-464b-a454-c00e2ecaa353	Access (0)	-NULL-	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={c2b34e61-07aa-4c2a-9f20-8287a1a32986}, Имя агента=null, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	\N	d05b2de9-68e7-4688-b8ab-2230ac87bc5e
f73256ce-f03c-4f71-a1c6-0efcb4474b73	LinkedPrimaryKey	-NULL-	{ccdfc400-223f-4960-b11f-fb6f632636ce}	dfe054d4-26ea-464b-a454-c00e2ecaa353	d05b2de9-68e7-4688-b8ab-2230ac87bc5e
7ca1b1f0-b85a-40d4-a767-426054f0c4a5	Access (1)	-NULL-	Access(Тип доступа=Read, Фильтр=null, Имя фильтра=null, Разрешение={c2b34e61-07aa-4c2a-9f20-8287a1a32986}, Имя агента=null, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	\N	d05b2de9-68e7-4688-b8ab-2230ac87bc5e
6e82ff94-3d56-47b2-9323-e02a3343e493	LinkedPrimaryKey	-NULL-	{68b1d2c8-4fbb-4f94-91a8-2fbe18e5d2c8}	7ca1b1f0-b85a-40d4-a767-426054f0c4a5	d05b2de9-68e7-4688-b8ab-2230ac87bc5e
aef8fb19-6b19-45b0-9297-f29b447156c0	TypeAccess	Read	-NULL-	\N	21a1eafc-88f5-4781-a7dc-86dd160c8e57
b55cf6a9-a10a-408e-bc8d-6bd313e60fbe	Filter	Filter(Имя фильтра=Чтение для отчётов)	-NULL-	\N	21a1eafc-88f5-4781-a7dc-86dd160c8e57
ab2e35ac-b55c-41fd-8339-df87d4b019f5	LinkedPrimaryKey	{feb7db2a-a2cc-42d3-a419-72e9da67ce5f}	-NULL-	b55cf6a9-a10a-408e-bc8d-6bd313e60fbe	21a1eafc-88f5-4781-a7dc-86dd160c8e57
38eb0d42-887c-4dc4-bff4-4a7f9b244ba1	Permition	Permition(Имя агента=Все пользователи, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	-NULL-	\N	21a1eafc-88f5-4781-a7dc-86dd160c8e57
0f7319ea-b419-4f72-a7ca-09b9f45241c8	LinkedPrimaryKey	{981c869c-dbde-4a11-bf66-779edcb58c96}	-NULL-	38eb0d42-887c-4dc4-bff4-4a7f9b244ba1	21a1eafc-88f5-4781-a7dc-86dd160c8e57
fba32c7d-13d2-4ef3-bc9b-517735d981dc	TypeAccess	Full	-NULL-	\N	4d7cac52-effc-4bca-a167-67ca35b53fdb
324c54f3-0cf5-4c4e-bc6a-72174ee5b5aa	Filter	Filter(Имя фильтра=Полный доступ к отчётам)	-NULL-	\N	4d7cac52-effc-4bca-a167-67ca35b53fdb
44187b24-9e50-42d3-93f2-1acf406bde42	LinkedPrimaryKey	{60e50089-a9d4-42af-9045-8c6fc999bb80}	-NULL-	324c54f3-0cf5-4c4e-bc6a-72174ee5b5aa	4d7cac52-effc-4bca-a167-67ca35b53fdb
2a0483c9-0ebc-43ef-a2a3-f91a67500b8c	Permition	Permition(Имя агента=Все пользователи, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	-NULL-	\N	4d7cac52-effc-4bca-a167-67ca35b53fdb
84930307-a6bc-4f02-bc05-cbaa4595e3ac	LinkedPrimaryKey	{981c869c-dbde-4a11-bf66-779edcb58c96}	-NULL-	2a0483c9-0ebc-43ef-a2a3-f91a67500b8c	4d7cac52-effc-4bca-a167-67ca35b53fdb
4e6ffb37-3f23-4eb1-8be4-732fc04296e9	TypeAccess	-NULL-	Full	\N	9a127ad5-81fa-4f52-9980-f20cb1efdb00
251f8fc2-8640-4b8e-828a-2970fa61447c	Filter	-NULL-	\N	\N	9a127ad5-81fa-4f52-9980-f20cb1efdb00
39d01569-084d-4cf8-96e6-dfd6ae9faaaa	Filter	-NULL-	\N	\N	9a127ad5-81fa-4f52-9980-f20cb1efdb00
071ce937-b02d-492c-b68f-b9d7ec9a5346	Permition	-NULL-	Permition(Имя агента=null, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	\N	9a127ad5-81fa-4f52-9980-f20cb1efdb00
5bb95fca-a941-4e73-93ed-7b8e97c0a892	LinkedPrimaryKey	-NULL-	{c2b34e61-07aa-4c2a-9f20-8287a1a32986}	071ce937-b02d-492c-b68f-b9d7ec9a5346	9a127ad5-81fa-4f52-9980-f20cb1efdb00
402ed9ad-b258-47c9-90fd-da31e8f3547c	TypeAccess	-NULL-	Read	\N	3299a62f-4e12-4c73-8fb8-d406112160dc
6ff5d3e1-d433-4a32-ad88-d63e33f6e5f2	Filter	-NULL-	\N	\N	3299a62f-4e12-4c73-8fb8-d406112160dc
f715f770-d1b8-4bb1-93fd-0a45c03f4f0e	Filter	-NULL-	\N	\N	3299a62f-4e12-4c73-8fb8-d406112160dc
7f2c5add-a783-439a-9693-004b87a5b1bd	Permition	-NULL-	Permition(Имя агента=null, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	\N	3299a62f-4e12-4c73-8fb8-d406112160dc
44b08020-7553-4092-adac-b61956ff52c3	LinkedPrimaryKey	-NULL-	{c2b34e61-07aa-4c2a-9f20-8287a1a32986}	7f2c5add-a783-439a-9693-004b87a5b1bd	3299a62f-4e12-4c73-8fb8-d406112160dc
ce0bd88f-ea56-48f5-9d48-c0b36ec653f8	Name	-NULL-	NewPlatform.Flexberry.HighwaySB.StatRecord	\N	d0c67a74-2c17-428f-ad25-3cc03eced9cc
fc5f409d-0809-49c2-92fc-76a2acd3c3a8	Type	-NULL-	\N	\N	d0c67a74-2c17-428f-ad25-3cc03eced9cc
1b2ad1f5-0cc3-4a98-a3c7-6aa1eba2c559	IsAttribute	-NULL-	False	\N	d0c67a74-2c17-428f-ad25-3cc03eced9cc
c511657c-60a3-4201-b0c3-de203cc4aa1f	IsOperation	-NULL-	False	\N	d0c67a74-2c17-428f-ad25-3cc03eced9cc
1548d6e3-f03c-4465-9445-a376294c0fff	IsView	-NULL-	False	\N	d0c67a74-2c17-428f-ad25-3cc03eced9cc
701d0a3a-d7a7-46fa-8d2d-9d0b48ca70fc	IsClass	-NULL-	True	\N	d0c67a74-2c17-428f-ad25-3cc03eced9cc
0f55b8dc-af15-4c99-b527-2614ff80144d	SharedOper	-NULL-	True	\N	d0c67a74-2c17-428f-ad25-3cc03eced9cc
8111ae5d-e8fd-4da2-ab12-f282d2631db4	Filter	-NULL-	\N	\N	91f675ad-97a5-4b17-b244-a708a777359e
35f36682-bd3f-4bd7-97cc-30b73a88ccdf	Name	-NULL-	NewPlatform.Flexberry.HighwaySB.ТипСообщения	\N	4eae13ea-b54a-48b3-b22d-2e77cb556e1e
68d5c675-cba1-4b4a-977f-de31316bc8f0	Type	-NULL-	\N	\N	4eae13ea-b54a-48b3-b22d-2e77cb556e1e
6f19653b-1381-411b-b2b6-cb933f581dbf	IsAttribute	-NULL-	False	\N	4eae13ea-b54a-48b3-b22d-2e77cb556e1e
2171fbcc-8f45-4118-8d4e-fb1363f2cb03	IsOperation	-NULL-	False	\N	4eae13ea-b54a-48b3-b22d-2e77cb556e1e
2566a438-d540-47ff-8635-978afef0cd9f	IsView	-NULL-	False	\N	4eae13ea-b54a-48b3-b22d-2e77cb556e1e
7e9fcd1d-2ebc-4df0-a639-e827053a8db3	IsClass	-NULL-	True	\N	4eae13ea-b54a-48b3-b22d-2e77cb556e1e
0792c338-1ea6-4138-b78c-c73df9a438cb	SharedOper	-NULL-	True	\N	4eae13ea-b54a-48b3-b22d-2e77cb556e1e
e7b122f6-0837-42c7-9a9a-108f3b374c15	Name	-NULL-	NewPlatform.Flexberry.HighwaySB.OutboundMessageTypeRestriction	\N	315f85fa-d1fe-4d61-b913-6da7a0f7e3f8
ecc4833b-8aa2-4630-a256-c5a4af862faf	Type	-NULL-	\N	\N	315f85fa-d1fe-4d61-b913-6da7a0f7e3f8
09c6e668-c2d5-423a-a794-da63d98cfc85	IsAttribute	-NULL-	False	\N	315f85fa-d1fe-4d61-b913-6da7a0f7e3f8
fc88aaa3-e89b-47d9-b8ac-195a6cc6f77a	IsOperation	-NULL-	False	\N	315f85fa-d1fe-4d61-b913-6da7a0f7e3f8
7fd9a220-1000-4615-9974-912f79e4d744	IsView	-NULL-	False	\N	315f85fa-d1fe-4d61-b913-6da7a0f7e3f8
a313fde8-207b-4079-a223-0e18e234b900	IsClass	-NULL-	True	\N	315f85fa-d1fe-4d61-b913-6da7a0f7e3f8
d3e6f4dc-1b1c-49d8-9df4-5bf8109141d2	SharedOper	-NULL-	True	\N	315f85fa-d1fe-4d61-b913-6da7a0f7e3f8
1593e196-1f91-4f4d-8d53-1acbee7a33f8	Name	-NULL-	NewPlatform.Flexberry.HighwaySB.Клиент	\N	c71a7eca-ff85-4c87-8c15-cbd21042d963
0c3025c7-1e57-4550-8965-7e0e4da72b57	Type	-NULL-	\N	\N	c71a7eca-ff85-4c87-8c15-cbd21042d963
75715cc8-3a92-48cd-acb4-37561b141dc5	IsAttribute	-NULL-	False	\N	c71a7eca-ff85-4c87-8c15-cbd21042d963
501abc7f-dcef-4775-852e-0461a7d5dcfe	IsOperation	-NULL-	False	\N	c71a7eca-ff85-4c87-8c15-cbd21042d963
537fcc41-3006-440a-b306-79cfc67a9c70	IsView	-NULL-	False	\N	c71a7eca-ff85-4c87-8c15-cbd21042d963
89c6daa3-3645-43e1-83ac-fd0878c83b0a	IsClass	-NULL-	True	\N	c71a7eca-ff85-4c87-8c15-cbd21042d963
d213f2e5-faca-4823-b1fd-378e4cbe3046	SharedOper	-NULL-	True	\N	c71a7eca-ff85-4c87-8c15-cbd21042d963
3a891f34-0c9f-4fb4-b3bf-a5f6a880e175	Name	-NULL-	NewPlatform.Flexberry.HighwaySB.StatSetting	\N	9f584772-1bb7-44e5-be75-c1620355fa75
c6793577-819a-4f55-b2a7-e906dddabbaf	Type	-NULL-	\N	\N	9f584772-1bb7-44e5-be75-c1620355fa75
202b596a-3baf-403a-898a-ef4d275bf689	IsAttribute	-NULL-	False	\N	9f584772-1bb7-44e5-be75-c1620355fa75
41d35db5-2e5b-41a8-903a-134daf39aa76	IsOperation	-NULL-	False	\N	9f584772-1bb7-44e5-be75-c1620355fa75
01615cb3-4b66-469a-a87f-7cc56dcfed2c	IsView	-NULL-	False	\N	9f584772-1bb7-44e5-be75-c1620355fa75
d808850b-11e8-4f22-9c15-13ff7c7b9854	IsClass	-NULL-	True	\N	9f584772-1bb7-44e5-be75-c1620355fa75
dbafa537-45a5-4326-a747-751422c10a16	SharedOper	-NULL-	True	\N	9f584772-1bb7-44e5-be75-c1620355fa75
3e618ede-f811-460b-ad49-57e1b80b092d	Name	-NULL-	NewPlatform.Flexberry.HighwaySB.Подписка	\N	af944103-26a7-43e4-a0a0-b629f79c6e17
122eb1c0-1c1c-48a0-bf09-c0cb79846c63	Type	-NULL-	\N	\N	af944103-26a7-43e4-a0a0-b629f79c6e17
33edabf0-9151-47c9-83f8-79db602c15b6	IsAttribute	-NULL-	False	\N	af944103-26a7-43e4-a0a0-b629f79c6e17
201292c8-65b5-495b-aba1-18b5b6780638	IsOperation	-NULL-	False	\N	af944103-26a7-43e4-a0a0-b629f79c6e17
d47cdf07-9b92-43e9-8859-dff301f64e00	IsView	-NULL-	False	\N	af944103-26a7-43e4-a0a0-b629f79c6e17
9ea55778-1980-4c03-8750-77b79fa99326	IsClass	-NULL-	True	\N	af944103-26a7-43e4-a0a0-b629f79c6e17
a0d13ab5-974f-4d75-9216-560e4b63c9bd	SharedOper	-NULL-	True	\N	af944103-26a7-43e4-a0a0-b629f79c6e17
01403b08-5c3d-4ace-ad36-ae4a4f355a41	Name	-NULL-	NewPlatform.Flexberry.HighwaySB.Сообщение	\N	d1d36116-0c00-4818-ae61-3562acae906e
1012bdf0-c26a-4456-b1a1-ae1858c3e607	Type	-NULL-	\N	\N	d1d36116-0c00-4818-ae61-3562acae906e
391f3347-04bd-47f2-83a4-cb9edb7bcd16	IsAttribute	-NULL-	False	\N	d1d36116-0c00-4818-ae61-3562acae906e
d645ddfe-8518-493a-b1c9-3189bacfab48	IsOperation	-NULL-	False	\N	d1d36116-0c00-4818-ae61-3562acae906e
443fca65-5546-48ca-a324-c846b751bd57	IsView	-NULL-	False	\N	d1d36116-0c00-4818-ae61-3562acae906e
029d2dcd-c708-43f5-aada-c6ac1bb28547	IsClass	-NULL-	True	\N	d1d36116-0c00-4818-ae61-3562acae906e
e1aa657e-2897-4477-b257-dbdf99b1f2a0	SharedOper	-NULL-	True	\N	d1d36116-0c00-4818-ae61-3562acae906e
6be88f97-2ca6-4ac3-8b59-4c9e7ac6f07d	Name	-NULL-	NewPlatform.Flexberry.HighwaySB.CompressionSetting	\N	3f98925f-10ee-45ff-91ca-baa6b00e1126
e478de13-fd95-484e-8b64-9d9d07314b08	Type	-NULL-	\N	\N	3f98925f-10ee-45ff-91ca-baa6b00e1126
86d77cd0-0d54-4798-86b3-ab8b5e798648	IsAttribute	-NULL-	False	\N	3f98925f-10ee-45ff-91ca-baa6b00e1126
7532eb59-6429-4b44-bdf2-9f9d6be96903	IsOperation	-NULL-	False	\N	3f98925f-10ee-45ff-91ca-baa6b00e1126
70cc8a9c-21f4-4ca4-aab3-0c96bbd61920	IsView	-NULL-	False	\N	3f98925f-10ee-45ff-91ca-baa6b00e1126
edb37edd-3bda-4e9c-a036-9d9acd4413f4	IsClass	-NULL-	True	\N	3f98925f-10ee-45ff-91ca-baa6b00e1126
3031fa18-8280-4c92-bd15-3e60837a338d	SharedOper	-NULL-	True	\N	3f98925f-10ee-45ff-91ca-baa6b00e1126
8aa90094-8c9a-446b-b4a9-121aa1666740	Agent	-NULL-	Agent(Имя агента=admin, Логин агента=null)	\N	eb04185e-6f45-4d55-9743-24eb7b1f68f2
22197cb1-c304-49d2-ad47-ee7f2a696a89	LinkedPrimaryKey	-NULL-	{590de25f-c7f0-4019-a53a-645f769b7b57}	8aa90094-8c9a-446b-b4a9-121aa1666740	eb04185e-6f45-4d55-9743-24eb7b1f68f2
165f37d6-bbd6-45ef-b986-b95925551819	Subject	-NULL-	Subject(Имя субъекта=null)	\N	eb04185e-6f45-4d55-9743-24eb7b1f68f2
34431dbe-5156-4495-a275-c9b9ac134e81	LinkedPrimaryKey	-NULL-	{1d742d0b-e6fd-4b56-8363-61c5f2178c45}	165f37d6-bbd6-45ef-b986-b95925551819	eb04185e-6f45-4d55-9743-24eb7b1f68f2
fd4c7393-0cdb-4ae0-ad35-9c35c7b06743	Access (0)	-NULL-	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={e495b619-89f6-4a03-8f11-8797ed477a3b}, Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	eb04185e-6f45-4d55-9743-24eb7b1f68f2
2eabf9c9-92f4-4b22-8e4a-c8f112ef291c	LinkedPrimaryKey	-NULL-	{2d3772cc-8eb3-4596-959c-dcc47eda189c}	fd4c7393-0cdb-4ae0-ad35-9c35c7b06743	eb04185e-6f45-4d55-9743-24eb7b1f68f2
8a210998-8dc8-4bd1-86e2-5cc8d857056c	Agent	-NULL-	Agent(Имя агента=admin, Логин агента=null)	\N	5bc0d754-4f14-4b02-b78d-9920b87862b3
f8153f91-1353-4e45-9916-78df11ec16f4	LinkedPrimaryKey	-NULL-	{590de25f-c7f0-4019-a53a-645f769b7b57}	8a210998-8dc8-4bd1-86e2-5cc8d857056c	5bc0d754-4f14-4b02-b78d-9920b87862b3
f39585e5-b87e-4430-b188-c09c5b8c6bc8	Subject	-NULL-	Subject(Имя субъекта=null)	\N	5bc0d754-4f14-4b02-b78d-9920b87862b3
5e99b1e1-35cf-45bb-968c-3f79aecd3c52	LinkedPrimaryKey	-NULL-	{816e18c6-79a6-4548-8695-37d9bc9d6f8c}	f39585e5-b87e-4430-b188-c09c5b8c6bc8	5bc0d754-4f14-4b02-b78d-9920b87862b3
a467c712-2f98-4889-9ac7-8e3b8129b86d	Access (0)	-NULL-	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={160e05bc-41e6-4e16-9135-16debc6515f6}, Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	5bc0d754-4f14-4b02-b78d-9920b87862b3
31acd5d4-c384-448f-82e3-1e3b7a3d0ba5	LinkedPrimaryKey	-NULL-	{a3765e56-2836-41cc-a0aa-9b098a2e2c48}	a467c712-2f98-4889-9ac7-8e3b8129b86d	5bc0d754-4f14-4b02-b78d-9920b87862b3
e1b150dc-15a8-43a2-a603-3f482bddb957	Agent	-NULL-	Agent(Имя агента=admin, Логин агента=null)	\N	4db0f195-b2eb-4108-886d-c9ae3f8692c9
624548bf-c55e-4340-948b-19b4eabd375d	LinkedPrimaryKey	-NULL-	{590de25f-c7f0-4019-a53a-645f769b7b57}	e1b150dc-15a8-43a2-a603-3f482bddb957	4db0f195-b2eb-4108-886d-c9ae3f8692c9
660613b1-1f2c-4095-97e6-f6c5a24c649f	Subject	-NULL-	Subject(Имя субъекта=null)	\N	4db0f195-b2eb-4108-886d-c9ae3f8692c9
94ad3770-3af2-4be8-8f3e-b0e98a77a1e6	LinkedPrimaryKey	-NULL-	{f4347bc7-6e1a-447c-8f87-87d8f34329ee}	660613b1-1f2c-4095-97e6-f6c5a24c649f	4db0f195-b2eb-4108-886d-c9ae3f8692c9
0a987c44-3615-4c5f-bbde-a4c6ea32949d	Access (0)	-NULL-	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={2057032d-f73b-4dcc-955b-bdbbb42174a9}, Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	4db0f195-b2eb-4108-886d-c9ae3f8692c9
e18bf174-82d0-481b-9e78-6a9d13829869	LinkedPrimaryKey	-NULL-	{842584b2-3cc0-4356-a9e9-d653845c8404}	0a987c44-3615-4c5f-bbde-a4c6ea32949d	4db0f195-b2eb-4108-886d-c9ae3f8692c9
8e690453-bace-4204-943b-6e75d8fa9072	Agent	-NULL-	Agent(Имя агента=admin, Логин агента=null)	\N	db880dfc-609f-4ef1-b42b-767f0d0e554c
b7ebd649-bc74-4247-b56a-131a891d85da	LinkedPrimaryKey	-NULL-	{590de25f-c7f0-4019-a53a-645f769b7b57}	8e690453-bace-4204-943b-6e75d8fa9072	db880dfc-609f-4ef1-b42b-767f0d0e554c
764ccb08-31c7-4e72-be2c-c1cbd976f3bb	Subject	-NULL-	Subject(Имя субъекта=null)	\N	db880dfc-609f-4ef1-b42b-767f0d0e554c
d5ab29fd-26ca-4414-97bd-e40534411927	LinkedPrimaryKey	-NULL-	{6fa6221e-c2f9-4a94-b560-b23575410bc2}	764ccb08-31c7-4e72-be2c-c1cbd976f3bb	db880dfc-609f-4ef1-b42b-767f0d0e554c
f06a28bc-24b6-4980-b4b7-deda011a890f	Filter	-NULL-	\N	\N	91f675ad-97a5-4b17-b244-a708a777359e
37017638-7661-4395-aaa7-e3778cf8dd50	Access (0)	-NULL-	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={83699e83-6d99-44e1-9023-fde55e00a5a8}, Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	db880dfc-609f-4ef1-b42b-767f0d0e554c
d0956700-b4e7-4769-93d0-7e3e61ea246a	LinkedPrimaryKey	-NULL-	{fc3be7fb-2c2d-4dcb-bf44-1079d28bd0ef}	37017638-7661-4395-aaa7-e3778cf8dd50	db880dfc-609f-4ef1-b42b-767f0d0e554c
104e722b-89e8-4973-87ca-d97c468cbd63	Agent	-NULL-	Agent(Имя агента=admin, Логин агента=null)	\N	64843b8d-b8aa-4e6f-8751-607e005be0c4
b60be161-7ab2-467f-af4d-06e897e1b432	LinkedPrimaryKey	-NULL-	{590de25f-c7f0-4019-a53a-645f769b7b57}	104e722b-89e8-4973-87ca-d97c468cbd63	64843b8d-b8aa-4e6f-8751-607e005be0c4
cf9e2884-084c-4a5b-a040-6d1e8fe95c89	Subject	-NULL-	Subject(Имя субъекта=null)	\N	64843b8d-b8aa-4e6f-8751-607e005be0c4
7787d664-298b-438b-a454-b75895a98d61	LinkedPrimaryKey	-NULL-	{76c27031-bc9a-4103-a1ad-cd33987097a6}	cf9e2884-084c-4a5b-a040-6d1e8fe95c89	64843b8d-b8aa-4e6f-8751-607e005be0c4
2d36ff44-8354-4086-bd61-17065f2f620c	Access (0)	-NULL-	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={910bafbc-6d23-4d40-8a11-6e8906351c78}, Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	64843b8d-b8aa-4e6f-8751-607e005be0c4
2e56a504-fe73-45a1-8156-efca48284d87	LinkedPrimaryKey	-NULL-	{67941ee4-8be3-4860-845c-8954c916dc24}	2d36ff44-8354-4086-bd61-17065f2f620c	64843b8d-b8aa-4e6f-8751-607e005be0c4
91f4c9f9-f628-4cf4-baff-64dcb74a9093	Agent	-NULL-	Agent(Имя агента=admin, Логин агента=null)	\N	de849d8f-3df6-4da0-9587-1a31f681a29b
2524d384-6f71-4272-8687-8a5da55104d3	LinkedPrimaryKey	-NULL-	{590de25f-c7f0-4019-a53a-645f769b7b57}	91f4c9f9-f628-4cf4-baff-64dcb74a9093	de849d8f-3df6-4da0-9587-1a31f681a29b
5a10c27b-ac77-4472-8bce-9bba86d908b4	Subject	-NULL-	Subject(Имя субъекта=null)	\N	de849d8f-3df6-4da0-9587-1a31f681a29b
246f226c-d4fa-4aa2-b76c-585460bec3ee	LinkedPrimaryKey	-NULL-	{c50c9d56-1a81-4b88-9c11-84559caa7f41}	5a10c27b-ac77-4472-8bce-9bba86d908b4	de849d8f-3df6-4da0-9587-1a31f681a29b
3731a6c7-d2d8-4d6d-9daf-8a7e18f3ff94	Access (0)	-NULL-	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={4520011a-23ac-4aa3-9ff9-71869aa9c5a4}, Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	de849d8f-3df6-4da0-9587-1a31f681a29b
b47b96be-9f55-4c08-9223-98a976f28287	LinkedPrimaryKey	-NULL-	{a38f056c-9c5d-4e77-8941-602e176cf796}	3731a6c7-d2d8-4d6d-9daf-8a7e18f3ff94	de849d8f-3df6-4da0-9587-1a31f681a29b
62effa70-f6a1-401d-b2a9-6c32dcc08d23	Agent	-NULL-	Agent(Имя агента=admin, Логин агента=null)	\N	a554cadb-2038-4b10-9186-da640c483e94
99772ce3-e18f-4a53-ba49-bbcd15c65e91	LinkedPrimaryKey	-NULL-	{590de25f-c7f0-4019-a53a-645f769b7b57}	62effa70-f6a1-401d-b2a9-6c32dcc08d23	a554cadb-2038-4b10-9186-da640c483e94
2c0d1dec-fd29-4aac-84d7-79b6342f1aa9	Subject	-NULL-	Subject(Имя субъекта=null)	\N	a554cadb-2038-4b10-9186-da640c483e94
0702888a-31b2-4975-ad3e-044bd220db08	LinkedPrimaryKey	-NULL-	{57a3968f-af04-4326-b72d-973bb7149e0f}	2c0d1dec-fd29-4aac-84d7-79b6342f1aa9	a554cadb-2038-4b10-9186-da640c483e94
8575bd68-1100-49e4-b074-09cc9a3ee394	Access (0)	-NULL-	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={d94545ac-c0f8-4671-b291-c784feeb56e0}, Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	a554cadb-2038-4b10-9186-da640c483e94
7c84f158-a684-45d3-a1b6-ed199039edc0	LinkedPrimaryKey	-NULL-	{3181b1c5-b8fb-4630-88d9-c776a3d1ce3f}	8575bd68-1100-49e4-b074-09cc9a3ee394	a554cadb-2038-4b10-9186-da640c483e94
2c49946e-6b9f-40cd-adb7-aeaa8cde9e19	Agent	-NULL-	Agent(Имя агента=admin, Логин агента=null)	\N	3760548c-c390-46ab-8b63-281c86027da0
79e82b8e-47e3-44ff-8168-7142599c04f6	LinkedPrimaryKey	-NULL-	{590de25f-c7f0-4019-a53a-645f769b7b57}	2c49946e-6b9f-40cd-adb7-aeaa8cde9e19	3760548c-c390-46ab-8b63-281c86027da0
b116bd30-93ae-42e7-ab70-a56bff2f380e	Subject	-NULL-	Subject(Имя субъекта=null)	\N	3760548c-c390-46ab-8b63-281c86027da0
fd8491ad-4533-4594-baff-cecacf2d3ee6	LinkedPrimaryKey	-NULL-	{9a39df06-e28e-4d44-a791-29cb705a0127}	b116bd30-93ae-42e7-ab70-a56bff2f380e	3760548c-c390-46ab-8b63-281c86027da0
60a0402a-217a-4c84-8fa4-cf3784817440	Access (0)	-NULL-	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={ecde3609-3bc3-4298-b50a-9dd61978b649}, Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	3760548c-c390-46ab-8b63-281c86027da0
af08252c-9941-49a5-aa4e-2f2b660a7516	LinkedPrimaryKey	-NULL-	{1ac0ce6b-8701-4cdb-b6c4-ffd16baabe9f}	60a0402a-217a-4c84-8fa4-cf3784817440	3760548c-c390-46ab-8b63-281c86027da0
8319063b-e2af-4043-83e5-c7e95fa1d891	Agent	-NULL-	Agent(Имя агента=admin, Логин агента=null)	\N	82316f25-e5e8-49c0-b0fe-521a0715a480
e713fd4b-0bc3-4e58-843f-552afd3a234b	LinkedPrimaryKey	-NULL-	{590de25f-c7f0-4019-a53a-645f769b7b57}	8319063b-e2af-4043-83e5-c7e95fa1d891	82316f25-e5e8-49c0-b0fe-521a0715a480
c9d02a28-f1de-43f9-bfa3-98f99462ce0b	Subject	-NULL-	Subject(Имя субъекта=null)	\N	82316f25-e5e8-49c0-b0fe-521a0715a480
bae8c2c7-ea7f-4532-8555-4dac04d19358	LinkedPrimaryKey	-NULL-	{37b65397-850e-445a-ad1b-ee34fa92eda2}	c9d02a28-f1de-43f9-bfa3-98f99462ce0b	82316f25-e5e8-49c0-b0fe-521a0715a480
7f17b1d8-92e7-48b5-a7b5-50172ae3c108	Access (0)	-NULL-	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={661f7ba3-3dea-4997-bb27-19e3553261d6}, Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	82316f25-e5e8-49c0-b0fe-521a0715a480
74755935-9250-4a02-906d-0d3055c140e9	LinkedPrimaryKey	-NULL-	{14350655-1f81-477e-b7ad-1ee9da5b08d6}	7f17b1d8-92e7-48b5-a7b5-50172ae3c108	82316f25-e5e8-49c0-b0fe-521a0715a480
5783433b-c445-4eb9-91bb-01a0deaf8e78	Agent	-NULL-	Agent(Имя агента=admin, Логин агента=null)	\N	a260d821-74d0-4718-8fd6-199333ab483b
accd0074-6989-440e-bfb9-14ff88d084b8	LinkedPrimaryKey	-NULL-	{590de25f-c7f0-4019-a53a-645f769b7b57}	5783433b-c445-4eb9-91bb-01a0deaf8e78	a260d821-74d0-4718-8fd6-199333ab483b
2733b0bf-108f-4182-a928-9e6be9464ea0	Subject	-NULL-	Subject(Имя субъекта=null)	\N	a260d821-74d0-4718-8fd6-199333ab483b
dfc37c24-a281-4fb2-9092-65c4ce5d1c32	LinkedPrimaryKey	-NULL-	{35963c75-7a21-49b4-9222-0e4dcd356a67}	2733b0bf-108f-4182-a928-9e6be9464ea0	a260d821-74d0-4718-8fd6-199333ab483b
40e9a63f-ef70-421a-a294-cf532ecddfdc	Access (0)	-NULL-	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={fcbed4cb-d102-4a17-9f12-6b7259b37e59}, Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	a260d821-74d0-4718-8fd6-199333ab483b
40caf18e-3fea-4c38-9638-3d965f31d2ea	LinkedPrimaryKey	-NULL-	{98879f07-29b7-4474-901b-05f6a018aec7}	40e9a63f-ef70-421a-a294-cf532ecddfdc	a260d821-74d0-4718-8fd6-199333ab483b
49773a00-4bcc-4a87-9075-76ddf65f0595	TypeAccess	-NULL-	Full	\N	d4a0b44b-df11-433c-8a62-a6f0e244830c
b4e98049-5ee0-40b9-b473-471f3508dd9f	Filter	-NULL-	\N	\N	d4a0b44b-df11-433c-8a62-a6f0e244830c
32eb27f1-ca7d-4737-b58d-fa98d3391f03	Filter	-NULL-	\N	\N	d4a0b44b-df11-433c-8a62-a6f0e244830c
76df8137-7cf8-4219-a727-a4696f3cd38a	Permition	-NULL-	Permition(Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	d4a0b44b-df11-433c-8a62-a6f0e244830c
595751bb-82c9-42c6-89e0-7922a07b17d1	LinkedPrimaryKey	-NULL-	{e495b619-89f6-4a03-8f11-8797ed477a3b}	76df8137-7cf8-4219-a727-a4696f3cd38a	d4a0b44b-df11-433c-8a62-a6f0e244830c
cbe46760-8956-4d4b-8174-9e6cfe1128fa	TypeAccess	-NULL-	Full	\N	83a08ad8-5ab4-412a-96a5-49d1c2eab051
ad44e8c0-f2ac-4eb1-b82a-e647eb78e3b4	Filter	-NULL-	\N	\N	83a08ad8-5ab4-412a-96a5-49d1c2eab051
1142b65f-b1c9-453e-9852-3a62d7b71729	Filter	-NULL-	\N	\N	83a08ad8-5ab4-412a-96a5-49d1c2eab051
1a7581dd-9ed8-40b1-9dc5-a578f8ee483e	Permition	-NULL-	Permition(Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	83a08ad8-5ab4-412a-96a5-49d1c2eab051
9cb1a7a6-78ec-455f-9088-0652f0dd036b	LinkedPrimaryKey	-NULL-	{160e05bc-41e6-4e16-9135-16debc6515f6}	1a7581dd-9ed8-40b1-9dc5-a578f8ee483e	83a08ad8-5ab4-412a-96a5-49d1c2eab051
70670a5a-834b-453c-8b8c-b21e24e3d403	TypeAccess	-NULL-	Full	\N	685b57f7-2cad-4ea3-b3d5-1054c1997a61
3a414d1c-912a-4d08-82ab-ef57c49d9389	Filter	-NULL-	\N	\N	685b57f7-2cad-4ea3-b3d5-1054c1997a61
01560600-2217-44ef-b49a-5fc8b8019cc2	Filter	-NULL-	\N	\N	685b57f7-2cad-4ea3-b3d5-1054c1997a61
4872ad00-90d8-4e9d-a2ae-8d23686e1c0a	Permition	-NULL-	Permition(Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	685b57f7-2cad-4ea3-b3d5-1054c1997a61
da627da9-7699-46f7-bfb7-9fcff8696d0e	LinkedPrimaryKey	-NULL-	{2057032d-f73b-4dcc-955b-bdbbb42174a9}	4872ad00-90d8-4e9d-a2ae-8d23686e1c0a	685b57f7-2cad-4ea3-b3d5-1054c1997a61
701455b3-95bd-4314-b297-903c91e8c07d	TypeAccess	-NULL-	Full	\N	91f675ad-97a5-4b17-b244-a708a777359e
cbbc6709-7d16-480e-aebd-797e9837aea7	Permition	-NULL-	Permition(Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	91f675ad-97a5-4b17-b244-a708a777359e
b936234f-be91-4ac2-8f18-a3f95f7a492d	LinkedPrimaryKey	-NULL-	{83699e83-6d99-44e1-9023-fde55e00a5a8}	cbbc6709-7d16-480e-aebd-797e9837aea7	91f675ad-97a5-4b17-b244-a708a777359e
b80d5833-1e9f-4102-a441-e1125fa62a4f	TypeAccess	-NULL-	Full	\N	0a1e956c-86e6-431c-a3b9-780e73d2ab7e
54960a3f-daef-460c-be98-2c9285bdc8a5	Filter	-NULL-	\N	\N	0a1e956c-86e6-431c-a3b9-780e73d2ab7e
91382ea0-3117-48b0-863a-b03817207539	Filter	-NULL-	\N	\N	0a1e956c-86e6-431c-a3b9-780e73d2ab7e
d1245b00-4594-49fb-9220-722743f75f16	Permition	-NULL-	Permition(Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	0a1e956c-86e6-431c-a3b9-780e73d2ab7e
48fd20e3-441c-4c34-9f3d-3ee5860a9bd8	LinkedPrimaryKey	-NULL-	{910bafbc-6d23-4d40-8a11-6e8906351c78}	d1245b00-4594-49fb-9220-722743f75f16	0a1e956c-86e6-431c-a3b9-780e73d2ab7e
37c7dd4f-c18a-45d6-ba21-4541a31bf0b5	TypeAccess	-NULL-	Full	\N	c105b2c6-6d9f-49c3-9035-19f48076eb9f
21125822-15fa-46aa-b2ae-c66f860cb1e9	Filter	-NULL-	\N	\N	c105b2c6-6d9f-49c3-9035-19f48076eb9f
40d5d3fb-94f5-4ca8-a4b5-514b16e10ed2	Filter	-NULL-	\N	\N	c105b2c6-6d9f-49c3-9035-19f48076eb9f
6d42fb4c-be93-4503-a864-949747f0fe40	Permition	-NULL-	Permition(Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	c105b2c6-6d9f-49c3-9035-19f48076eb9f
bb5dffd7-f043-47a2-a17a-d7402146ab3c	LinkedPrimaryKey	-NULL-	{4520011a-23ac-4aa3-9ff9-71869aa9c5a4}	6d42fb4c-be93-4503-a864-949747f0fe40	c105b2c6-6d9f-49c3-9035-19f48076eb9f
c95f1128-fe89-4e48-984f-23f410392399	TypeAccess	-NULL-	Full	\N	7be5d286-c862-45d6-94a2-f64d91874183
4d842db6-dfd5-46ef-9f60-606acf867dc2	Filter	-NULL-	\N	\N	7be5d286-c862-45d6-94a2-f64d91874183
1843ccca-50b4-458e-9ffd-7a2889f401bf	Filter	-NULL-	\N	\N	7be5d286-c862-45d6-94a2-f64d91874183
1b3d815d-6d9a-4301-9560-48a8e212c3a0	Permition	-NULL-	Permition(Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	7be5d286-c862-45d6-94a2-f64d91874183
6d995326-811a-4a6e-99a4-28f22fafa44c	LinkedPrimaryKey	-NULL-	{d94545ac-c0f8-4671-b291-c784feeb56e0}	1b3d815d-6d9a-4301-9560-48a8e212c3a0	7be5d286-c862-45d6-94a2-f64d91874183
088cee23-3d6a-4d50-bbb0-edb0a4b486d0	TypeAccess	-NULL-	Full	\N	e48ad3a9-e85f-4aad-adfb-1d5cb16ec9f6
08344faf-b177-4448-80d7-acc633be2af6	Filter	-NULL-	\N	\N	e48ad3a9-e85f-4aad-adfb-1d5cb16ec9f6
d443e9c6-a126-4545-b365-44834e6dad70	Filter	-NULL-	\N	\N	e48ad3a9-e85f-4aad-adfb-1d5cb16ec9f6
856ad1f7-08c8-49e7-9a13-c888ae272e73	Permition	-NULL-	Permition(Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	e48ad3a9-e85f-4aad-adfb-1d5cb16ec9f6
b1a0cca3-e3f6-4e5c-9738-fa3e33db5c79	LinkedPrimaryKey	-NULL-	{ecde3609-3bc3-4298-b50a-9dd61978b649}	856ad1f7-08c8-49e7-9a13-c888ae272e73	e48ad3a9-e85f-4aad-adfb-1d5cb16ec9f6
001baee0-8dce-4297-b146-897c451867e4	TypeAccess	-NULL-	Full	\N	6152204e-228f-4bf1-bc8e-0cf84bb926ce
d68653f9-cb9f-48da-8391-36beda50d06d	Filter	-NULL-	\N	\N	6152204e-228f-4bf1-bc8e-0cf84bb926ce
21250268-a2af-4b9c-8fbb-96ea4aeab8f8	Filter	-NULL-	\N	\N	6152204e-228f-4bf1-bc8e-0cf84bb926ce
e60e3039-0e1f-484c-8102-5327dd787138	Permition	-NULL-	Permition(Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	6152204e-228f-4bf1-bc8e-0cf84bb926ce
cc804c24-7717-455f-9db0-66a7dc25e7fb	LinkedPrimaryKey	-NULL-	{661f7ba3-3dea-4997-bb27-19e3553261d6}	e60e3039-0e1f-484c-8102-5327dd787138	6152204e-228f-4bf1-bc8e-0cf84bb926ce
406e8f6d-e034-40d4-a26b-ce0a0c12e762	TypeAccess	-NULL-	Full	\N	b64e602a-2df0-4650-980b-b92a4b247bde
8bf835b6-b2db-4142-b33a-f977ffaead8e	Filter	-NULL-	\N	\N	b64e602a-2df0-4650-980b-b92a4b247bde
21e34c7e-a610-4759-b846-142ca84955a9	Filter	-NULL-	\N	\N	b64e602a-2df0-4650-980b-b92a4b247bde
28b1c0ab-d991-4350-b955-16d44e172fd6	Permition	-NULL-	Permition(Имя агента=admin, Логи агента=null, Имя субъекта=null)	\N	b64e602a-2df0-4650-980b-b92a4b247bde
519a4d90-71c9-4318-935c-4d56a65ea964	LinkedPrimaryKey	-NULL-	{fcbed4cb-d102-4a17-9f12-6b7259b37e59}	28b1c0ab-d991-4350-b955-16d44e172fd6	b64e602a-2df0-4650-980b-b92a4b247bde
d43ee77b-c853-43b8-ba53-f83c167db55e	Agent	Agent(Имя агента=Все пользователи, Логин агента=null)	-NULL-	\N	87c6977c-4928-4686-a450-b300818504f6
6c3107d1-dfe6-424d-af8a-3ce7c9ded136	LinkedPrimaryKey	{7b9cee01-bef5-4cbb-88b0-77ab212ae881}	-NULL-	d43ee77b-c853-43b8-ba53-f83c167db55e	87c6977c-4928-4686-a450-b300818504f6
50e60550-3682-41e1-883f-b4156fc3fcdb	Subject	Subject(Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	-NULL-	\N	87c6977c-4928-4686-a450-b300818504f6
8e51d86b-51ac-4063-9675-5d6e3f9ce830	LinkedPrimaryKey	{1d742d0b-e6fd-4b56-8363-61c5f2178c45}	-NULL-	50e60550-3682-41e1-883f-b4156fc3fcdb	87c6977c-4928-4686-a450-b300818504f6
a6c13f93-f55f-4b14-8620-72088c99aa98	Access (0)	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={c2b34e61-07aa-4c2a-9f20-8287a1a32986}, Имя агента=Все пользователи, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	-NULL-	\N	87c6977c-4928-4686-a450-b300818504f6
4510a5c0-fe15-4be2-87f6-3f36510f01d6	LinkedPrimaryKey	{ccdfc400-223f-4960-b11f-fb6f632636ce}	-NULL-	a6c13f93-f55f-4b14-8620-72088c99aa98	87c6977c-4928-4686-a450-b300818504f6
65bd2b80-5db8-4e45-8dfc-e47f56f45151	Access (1)	Access(Тип доступа=Read, Фильтр=null, Имя фильтра=null, Разрешение={c2b34e61-07aa-4c2a-9f20-8287a1a32986}, Имя агента=Все пользователи, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	-NULL-	\N	87c6977c-4928-4686-a450-b300818504f6
f0894138-b862-4e71-8f37-e64cacf41f66	LinkedPrimaryKey	{68b1d2c8-4fbb-4f94-91a8-2fbe18e5d2c8}	-NULL-	65bd2b80-5db8-4e45-8dfc-e47f56f45151	87c6977c-4928-4686-a450-b300818504f6
859d75cd-b15a-496c-aef8-318ae27df6e4	Agent	Agent(Имя агента=admin, Логин агента=null)	-NULL-	\N	bd02141c-463a-4e0a-ad85-216eaba345d6
a4594c63-5b64-4eec-84c7-42a7528ac4bc	LinkedPrimaryKey	{590de25f-c7f0-4019-a53a-645f769b7b57}	-NULL-	859d75cd-b15a-496c-aef8-318ae27df6e4	bd02141c-463a-4e0a-ad85-216eaba345d6
b3d83348-51a7-4007-a633-c9d938ab582b	Subject	Subject(Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	-NULL-	\N	bd02141c-463a-4e0a-ad85-216eaba345d6
c0d34c61-c8ea-4ef3-b4a6-e201efffdc73	LinkedPrimaryKey	{1d742d0b-e6fd-4b56-8363-61c5f2178c45}	-NULL-	b3d83348-51a7-4007-a633-c9d938ab582b	bd02141c-463a-4e0a-ad85-216eaba345d6
431fe1a0-0342-49a9-83bc-a2583a43b15f	Access (0)	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={e495b619-89f6-4a03-8f11-8797ed477a3b}, Имя агента=admin, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	-NULL-	\N	bd02141c-463a-4e0a-ad85-216eaba345d6
98a7c6dc-0248-4609-9df8-fe3d25bf036d	LinkedPrimaryKey	{2d3772cc-8eb3-4596-959c-dcc47eda189c}	-NULL-	431fe1a0-0342-49a9-83bc-a2583a43b15f	bd02141c-463a-4e0a-ad85-216eaba345d6
7d2bf886-48e1-44c4-83e4-d982123b0623	Agent	-NULL-	Agent(Имя агента=null, Логин агента=null)	\N	47294617-a308-487d-915b-fdae398109e4
32457979-bfd9-49ba-8f81-38a192acb675	LinkedPrimaryKey	-NULL-	{590de25f-c7f0-4019-a53a-645f769b7b57}	7d2bf886-48e1-44c4-83e4-d982123b0623	47294617-a308-487d-915b-fdae398109e4
ceb87673-7e33-4250-a001-3db612fabcb0	Subject	-NULL-	Subject(Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	\N	47294617-a308-487d-915b-fdae398109e4
84b01f5f-33d2-4f27-b070-9c836433f0fd	LinkedPrimaryKey	-NULL-	{1d742d0b-e6fd-4b56-8363-61c5f2178c45}	ceb87673-7e33-4250-a001-3db612fabcb0	47294617-a308-487d-915b-fdae398109e4
1b1e19a9-a9eb-47b2-8d1c-d620dce7e53b	Access (0)	-NULL-	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={1d099001-49f4-4c40-a078-54a59f63f2c9}, Имя агента=null, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	\N	47294617-a308-487d-915b-fdae398109e4
9d5a099b-5908-4e96-9004-809f188f325b	LinkedPrimaryKey	-NULL-	{b555c81b-be06-4bc5-897a-b590e6722a22}	1b1e19a9-a9eb-47b2-8d1c-d620dce7e53b	47294617-a308-487d-915b-fdae398109e4
fe9648e0-c7ba-4f94-815a-ead4b2a789e7	Agent	-NULL-	Agent(Имя агента=null, Логин агента=null)	\N	9ada43ef-6506-463c-96f4-086fef4b8be5
59c9807a-5aa9-4cd9-8ac6-ad2654875e9e	LinkedPrimaryKey	-NULL-	{7b9cee01-bef5-4cbb-88b0-77ab212ae881}	fe9648e0-c7ba-4f94-815a-ead4b2a789e7	9ada43ef-6506-463c-96f4-086fef4b8be5
e3ddcf0a-5eee-4b2e-ae49-f9e29d6fe6c0	Subject	-NULL-	Subject(Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	\N	9ada43ef-6506-463c-96f4-086fef4b8be5
72fdbda2-a43e-4356-bff5-593ccdb70d3e	LinkedPrimaryKey	-NULL-	{1d742d0b-e6fd-4b56-8363-61c5f2178c45}	e3ddcf0a-5eee-4b2e-ae49-f9e29d6fe6c0	9ada43ef-6506-463c-96f4-086fef4b8be5
c95b573c-22ae-482a-8030-6c219ce0cb50	Access (0)	-NULL-	Access(Тип доступа=Full, Фильтр=null, Имя фильтра=null, Разрешение={44e81883-e2d4-4f68-9be6-dfa5eb937545}, Имя агента=null, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	\N	9ada43ef-6506-463c-96f4-086fef4b8be5
26fde52f-ec2a-49dd-b044-06548c3ccacd	LinkedPrimaryKey	-NULL-	{d4e5a515-050d-4bec-981f-c98f308882ac}	c95b573c-22ae-482a-8030-6c219ce0cb50	9ada43ef-6506-463c-96f4-086fef4b8be5
1506af56-3f1e-4376-8d97-94649cbf0074	Access (1)	-NULL-	Access(Тип доступа=Read, Фильтр=null, Имя фильтра=null, Разрешение={44e81883-e2d4-4f68-9be6-dfa5eb937545}, Имя агента=null, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	\N	9ada43ef-6506-463c-96f4-086fef4b8be5
e1d39b41-ba03-468a-871a-13fbc4134635	LinkedPrimaryKey	-NULL-	{53f595b5-8aea-4ccb-9309-18ff79048c93}	1506af56-3f1e-4376-8d97-94649cbf0074	9ada43ef-6506-463c-96f4-086fef4b8be5
0eaaa330-ea92-4e73-8825-f62d06f17b56	TypeAccess	Full	-NULL-	\N	af33161a-5efb-450a-82eb-3cb5e36b4b54
4b5bc5a4-74b5-4e81-82d3-c8087e7bfc5d	Filter	\N	-NULL-	\N	af33161a-5efb-450a-82eb-3cb5e36b4b54
c1d9b9d0-cb7c-4413-8809-b0873171cf0f	Filter	\N	-NULL-	\N	af33161a-5efb-450a-82eb-3cb5e36b4b54
06f0a12e-2b95-4dcb-9041-c760fe58530e	Permition	Permition(Имя агента=Все пользователи, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	-NULL-	\N	af33161a-5efb-450a-82eb-3cb5e36b4b54
e6539ef1-17c5-49dc-adca-c924db53e159	LinkedPrimaryKey	{c2b34e61-07aa-4c2a-9f20-8287a1a32986}	-NULL-	06f0a12e-2b95-4dcb-9041-c760fe58530e	af33161a-5efb-450a-82eb-3cb5e36b4b54
89439e72-c0f2-488d-9fa5-529249906722	TypeAccess	Read	-NULL-	\N	04db988e-ab00-452f-8d22-7e0299ebfdb7
ae5e75c3-8c1a-4ef2-802b-311947394668	Filter	\N	-NULL-	\N	04db988e-ab00-452f-8d22-7e0299ebfdb7
4b03008d-311c-4b42-926b-53d22dc255d2	Filter	\N	-NULL-	\N	04db988e-ab00-452f-8d22-7e0299ebfdb7
b372b02e-4f37-466b-8617-f7b03a1f1c9c	Permition	Permition(Имя агента=Все пользователи, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	-NULL-	\N	04db988e-ab00-452f-8d22-7e0299ebfdb7
9e4b7099-f920-4f65-9d4d-8c0fbb080be9	LinkedPrimaryKey	{c2b34e61-07aa-4c2a-9f20-8287a1a32986}	-NULL-	b372b02e-4f37-466b-8617-f7b03a1f1c9c	04db988e-ab00-452f-8d22-7e0299ebfdb7
ef135914-68ca-47fc-95ce-b93cb41f0ff4	TypeAccess	Full	-NULL-	\N	27997d66-2495-4425-9a50-f5b74d71566d
358f279a-1854-483b-b6e7-01d31c52cf00	Filter	\N	-NULL-	\N	27997d66-2495-4425-9a50-f5b74d71566d
b106108c-84c2-47a3-b545-c45f27c2b3ab	Filter	\N	-NULL-	\N	27997d66-2495-4425-9a50-f5b74d71566d
42f76c60-0890-41b5-8df8-b579c020f460	Permition	Permition(Имя агента=admin, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	-NULL-	\N	27997d66-2495-4425-9a50-f5b74d71566d
9abebcef-7c8b-4ab4-b893-a4ef1692eb5e	LinkedPrimaryKey	{e495b619-89f6-4a03-8f11-8797ed477a3b}	-NULL-	42f76c60-0890-41b5-8df8-b579c020f460	27997d66-2495-4425-9a50-f5b74d71566d
af992940-a642-4fce-bad0-00d654ba6e0d	TypeAccess	-NULL-	Full	\N	aa316bc3-2bd7-427a-8ae6-2a24c9e5b171
95775dc4-050d-4213-8726-8b5e42baed6a	Filter	-NULL-	\N	\N	aa316bc3-2bd7-427a-8ae6-2a24c9e5b171
0e10a8cb-ae6d-4231-bb39-06b92dbc2784	Filter	-NULL-	\N	\N	aa316bc3-2bd7-427a-8ae6-2a24c9e5b171
4cba3704-188c-46e4-8abd-d2d9ebe31b2f	Permition	-NULL-	Permition(Имя агента=null, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	\N	aa316bc3-2bd7-427a-8ae6-2a24c9e5b171
0ab51ecf-2840-4f7b-9224-19c95d239ed4	LinkedPrimaryKey	-NULL-	{1d099001-49f4-4c40-a078-54a59f63f2c9}	4cba3704-188c-46e4-8abd-d2d9ebe31b2f	aa316bc3-2bd7-427a-8ae6-2a24c9e5b171
f0fa28c5-0a2e-4b0a-905d-0b8cfa06e8fe	TypeAccess	-NULL-	Full	\N	0110d4df-0532-49df-b4b4-b98104517faf
8de62e37-b849-48b6-8e9f-a891b40c9335	Filter	-NULL-	\N	\N	0110d4df-0532-49df-b4b4-b98104517faf
31648375-306f-40f6-a6e2-001a77b3644f	Filter	-NULL-	\N	\N	0110d4df-0532-49df-b4b4-b98104517faf
39abcaf4-77cd-473e-8e78-9832cc6a002c	Permition	-NULL-	Permition(Имя агента=null, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	\N	0110d4df-0532-49df-b4b4-b98104517faf
7c006113-1410-4b69-8ab5-079538030105	LinkedPrimaryKey	-NULL-	{44e81883-e2d4-4f68-9be6-dfa5eb937545}	39abcaf4-77cd-473e-8e78-9832cc6a002c	0110d4df-0532-49df-b4b4-b98104517faf
6d6f0f72-ab4f-4f68-a257-71235bd85f2e	TypeAccess	-NULL-	Read	\N	629a10f0-3be0-4dc5-82b9-6a7263a7cd9d
243d68a0-33ac-4160-9c2a-c51fd5afa91e	Filter	-NULL-	\N	\N	629a10f0-3be0-4dc5-82b9-6a7263a7cd9d
b6c9be65-184d-4dae-8a09-ce00bd43a3ca	Filter	-NULL-	\N	\N	629a10f0-3be0-4dc5-82b9-6a7263a7cd9d
32eee539-5dc2-4d26-9bbb-3ad0fe891b18	Permition	-NULL-	Permition(Имя агента=null, Логи агента=null, Имя субъекта=ICSSoft.STORMNET.Reports.StormReport)	\N	629a10f0-3be0-4dc5-82b9-6a7263a7cd9d
e3da334a-3a9c-4ca5-bf8b-d90284a1dbda	LinkedPrimaryKey	-NULL-	{44e81883-e2d4-4f68-9be6-dfa5eb937545}	32eee539-5dc2-4d26-9bbb-3ad0fe891b18	629a10f0-3be0-4dc5-82b9-6a7263a7cd9d
\.


--
-- TOC entry 3187 (class 0 OID 16821)
-- Dependencies: 232
-- Data for Name: stormauobjtype; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormauobjtype (primarykey, name) FROM stdin;
24e78568-10c1-424b-be66-d0840151ad13	ICSSoft.STORMNET.Security.LinkRole, NewPlatform.Flexberry.Security.Objects, Version=1.0.0.1, Culture=neutral, PublicKeyToken=e9e142aba0117561
26cd08ef-1419-40d7-9396-1e51d9a6d8ab	ICSSoft.STORMNET.Security.Agent, NewPlatform.Flexberry.Security.Objects, Version=1.0.0.1, Culture=neutral, PublicKeyToken=e9e142aba0117561
cd701c42-5686-4dae-b87d-806138bd8c57	ICSSoft.STORMNET.Security.Permition, NewPlatform.Flexberry.Security.Objects, Version=1.0.0.1, Culture=neutral, PublicKeyToken=e9e142aba0117561
b477f9c9-ab2d-42ce-9acf-7fb4e6a41c23	ICSSoft.STORMNET.Security.Access, NewPlatform.Flexberry.Security.Objects, Version=1.0.0.1, Culture=neutral, PublicKeyToken=e9e142aba0117561
f33336ec-f5c6-40e5-81e0-54a273deef06	ICSSoft.STORMNET.Security.Subject, NewPlatform.Flexberry.Security.Objects, Version=1.0.0.1, Culture=neutral, PublicKeyToken=e9e142aba0117561
\.


--
-- TOC entry 3143 (class 0 OID 16402)
-- Dependencies: 188
-- Data for Name: stormf; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormf (primarykey, filtertext, name, filtertypenview, subject_m0, createtime, creator, edittime, editor) FROM stdin;
feb7db2a-a2cc-42d3-a419-72e9da67ce5f	AAEAAAD/////AQAAAAAAAAAQAQAAAAMAAAAICAwAAAAJAgAAAAkDAAAABAIAAAAcU3lzdGVtLkNvbGxlY3Rpb25zLkFycmF5TGlzdAMAAAAGX2l0ZW1zBV9zaXplCF92ZXJzaW9uBQAACAgJBAAAAAIAAAACAAAAAQMAAAACAAAACQUAAAACAAAAAgAAABAEAAAABAAAAAkGAAAACQcAAAANAhAFAAAABAAAAAYIAAAABEZ1bmMGCQAAAJcBSUNTU29mdC5TVE9STU5FVC5GdW5jdGlvbmFsTGFuZ3VhZ2UuVmFyaWFibGVEZWYsIElDU1NvZnQuU1RPUk1ORVQuRnVuY3Rpb25hbExhbmd1YWdlLCBWZXJzaW9uPTEuMC4wLjEsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49ODhlZmM2Zjc2ZTVjZDE2MQ0CEAYAAAADAAAACAgkAAAACQoAAAAJCwAAABEHAAAAAwAAAAYMAAAAB0Jvb2xlYW4GDQAAAAhJc1NoYXJlZAYOAAAACtCe0LHRidC40LkBCgAAAAIAAAAJDwAAAAIAAAACAAAAAQsAAAACAAAACRAAAAACAAAAAgAAABAPAAAABAAAAAkRAAAACRIAAAANAhAQAAAABAAAAAYTAAAAlwFJQ1NTb2Z0LlNUT1JNTkVULkZ1bmN0aW9uYWxMYW5ndWFnZS5WYXJpYWJsZURlZiwgSUNTU29mdC5TVE9STU5FVC5GdW5jdGlvbmFsTGFuZ3VhZ2UsIFZlcnNpb249MS4wLjAuMSwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj04OGVmYzZmNzZlNWNkMTYxCQgAAAANAhERAAAAAwAAAAYVAAAABlN0cmluZwYWAAAABU93bmVyBhcAAAAQ0JLQu9Cw0LTQtdC70LXRhhASAAAAAwAAAAgIjgAAAAkYAAAACRkAAAABGAAAAAIAAAAJGgAAAAAAAAAAAAAAARkAAAACAAAACRoAAAAAAAAAAAAAABAaAAAAAAAAAAs=	Чтение для отчётов	\N	\N	\N	\N	\N	\N
60e50089-a9d4-42af-9045-8c6fc999bb80	AAEAAAD/////AQAAAAAAAAAQAQAAAAMAAAAICA0AAAAJAgAAAAkDAAAABAIAAAAcU3lzdGVtLkNvbGxlY3Rpb25zLkFycmF5TGlzdAMAAAAGX2l0ZW1zBV9zaXplCF92ZXJzaW9uBQAACAgJBAAAAAEAAAABAAAAAQMAAAACAAAACQUAAAABAAAAAQAAABAEAAAABAAAAAkGAAAADQMQBQAAAAQAAAAGBwAAAARGdW5jDQMQBgAAAAMAAAAICCQAAAAJCAAAAAkJAAAAAQgAAAACAAAACQoAAAACAAAAAgAAAAEJAAAAAgAAAAkLAAAAAgAAAAIAAAAQCgAAAAQAAAAJDAAAAAkNAAAADQIQCwAAAAQAAAAGDgAAAJcBSUNTU29mdC5TVE9STU5FVC5GdW5jdGlvbmFsTGFuZ3VhZ2UuVmFyaWFibGVEZWYsIElDU1NvZnQuU1RPUk1ORVQuRnVuY3Rpb25hbExhbmd1YWdlLCBWZXJzaW9uPTEuMC4wLjEsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49ODhlZmM2Zjc2ZTVjZDE2MQkHAAAADQIRDAAAAAMAAAAGEAAAAAZTdHJpbmcGEQAAAAVPd25lcgYSAAAAENCS0LvQsNC00LXQu9C10YYQDQAAAAMAAAAICI4AAAAJEwAAAAkUAAAAARMAAAACAAAACRUAAAAAAAAAAAAAAAEUAAAAAgAAAAkVAAAAAAAAAAAAAAAQFQAAAAAAAAAL	Полный доступ к отчётам	\N	\N	\N	\N	\N	\N
bea91ee7-ab73-484f-9bf0-f3a476f55288	AAEAAAD/////AQAAAAAAAAAQAQAAAAMAAAAICAwAAAAJAgAAAAkDAAAABAIAAAAcU3lzdGVtLkNvbGxlY3Rpb25zLkFycmF5TGlzdAMAAAAGX2l0ZW1zBV9zaXplCF92ZXJzaW9uBQAACAgJBAAAAAIAAAACAAAAAQMAAAACAAAACQUAAAACAAAAAgAAABAEAAAABAAAAAkGAAAACQcAAAANAhAFAAAABAAAAAYIAAAAlwFJQ1NTb2Z0LlNUT1JNTkVULkZ1bmN0aW9uYWxMYW5ndWFnZS5WYXJpYWJsZURlZiwgSUNTU29mdC5TVE9STU5FVC5GdW5jdGlvbmFsTGFuZ3VhZ2UsIFZlcnNpb249MS4wLjAuMSwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj04OGVmYzZmNzZlNWNkMTYxBgkAAAAERnVuYw0CEQYAAAADAAAABgoAAAAHQm9vbGVhbgYLAAAACElzU2hhcmVkBgwAAAAK0J7QsdGJ0LjQuRAHAAAAAwAAAAgIJAAAAAkNAAAACQ4AAAABDQAAAAIAAAAJDwAAAAIAAAACAAAAAQ4AAAACAAAACRAAAAACAAAAAgAAABAPAAAABAAAAAkRAAAACRIAAAANAhAQAAAABAAAAAYTAAAAlwFJQ1NTb2Z0LlNUT1JNTkVULkZ1bmN0aW9uYWxMYW5ndWFnZS5WYXJpYWJsZURlZiwgSUNTU29mdC5TVE9STU5FVC5GdW5jdGlvbmFsTGFuZ3VhZ2UsIFZlcnNpb249MS4wLjAuMSwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj04OGVmYzZmNzZlNWNkMTYxCQkAAAANAhERAAAAAwAAAAYVAAAABlN0cmluZwYWAAAABU93bmVyBhcAAAAQ0JLQu9Cw0LTQtdC70LXRhhASAAAAAwAAAAgIjgAAAAkYAAAACRkAAAABGAAAAAIAAAAJGgAAAAAAAAAAAAAAARkAAAACAAAACRoAAAAAAAAAAAAAABAaAAAAAAAAAAs=	Чтение для документов отчётов	\N	\N	\N	\N	\N	\N
86954df9-c193-4517-b81c-ae8b4dea7420	AAEAAAD/////AQAAAAAAAAAQAQAAAAMAAAAICA0AAAAJAgAAAAkDAAAABAIAAAAcU3lzdGVtLkNvbGxlY3Rpb25zLkFycmF5TGlzdAMAAAAGX2l0ZW1zBV9zaXplCF92ZXJzaW9uBQAACAgJBAAAAAEAAAABAAAAAQMAAAACAAAACQUAAAABAAAAAQAAABAEAAAABAAAAAkGAAAADQMQBQAAAAQAAAAGBwAAAARGdW5jDQMQBgAAAAMAAAAICCQAAAAJCAAAAAkJAAAAAQgAAAACAAAACQoAAAACAAAAAgAAAAEJAAAAAgAAAAkLAAAAAgAAAAIAAAAQCgAAAAQAAAAJDAAAAAkNAAAADQIQCwAAAAQAAAAGDgAAAJcBSUNTU29mdC5TVE9STU5FVC5GdW5jdGlvbmFsTGFuZ3VhZ2UuVmFyaWFibGVEZWYsIElDU1NvZnQuU1RPUk1ORVQuRnVuY3Rpb25hbExhbmd1YWdlLCBWZXJzaW9uPTEuMC4wLjEsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49ODhlZmM2Zjc2ZTVjZDE2MQkHAAAADQIRDAAAAAMAAAAGEAAAAAZTdHJpbmcGEQAAAAVPd25lcgYSAAAAENCS0LvQsNC00LXQu9C10YYQDQAAAAMAAAAICI4AAAAJEwAAAAkUAAAAARMAAAACAAAACRUAAAAAAAAAAAAAAAEUAAAAAgAAAAkVAAAAAAAAAAAAAAAQFQAAAAAAAAAL	Полный доступ для документов отчётов	\N	\N	\N	\N	\N	\N
\.


--
-- TOC entry 3183 (class 0 OID 16789)
-- Dependencies: 228
-- Data for Name: stormfilterdetail; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormfilterdetail (primarykey, caption, dataobjectview, connectmasterprop, ownerconnectprop, filtersetting_m0) FROM stdin;
\.


--
-- TOC entry 3184 (class 0 OID 16797)
-- Dependencies: 229
-- Data for Name: stormfilterlookup; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormfilterlookup (primarykey, dataobjecttype, container, containertag, fieldstoview, filtersetting_m0) FROM stdin;
\.


--
-- TOC entry 3181 (class 0 OID 16773)
-- Dependencies: 226
-- Data for Name: stormfiltersetting; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormfiltersetting (primarykey, name, dataobjectview) FROM stdin;
\.


--
-- TOC entry 3144 (class 0 OID 16408)
-- Dependencies: 189
-- Data for Name: stormi; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormi (primarykey, user_m0, agent_m0, createtime, creator, edittime, editor) FROM stdin;
\.


--
-- TOC entry 3145 (class 0 OID 16414)
-- Dependencies: 190
-- Data for Name: stormla; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormla (primarykey, view_m0, attribute_m0, createtime, creator, edittime, editor) FROM stdin;
\.


--
-- TOC entry 3146 (class 0 OID 16420)
-- Dependencies: 191
-- Data for Name: stormlg; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormlg (primarykey, group_m0, user_m0, createtime, creator, edittime, editor) FROM stdin;
\.


--
-- TOC entry 3147 (class 0 OID 16426)
-- Dependencies: 192
-- Data for Name: stormlo; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormlo (primarykey, class_m0, operation_m0, createtime, creator, edittime, editor) FROM stdin;
\.


--
-- TOC entry 3148 (class 0 OID 16432)
-- Dependencies: 193
-- Data for Name: stormlr; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormlr (primarykey, startdate, enddate, agent_m0, role_m0, createtime, creator, edittime, editor) FROM stdin;
61918f08-46b7-426a-98b9-4f4d008de1c4	\N	\N	082345bb-ba23-445c-a8a1-10b8ab84748b	590de25f-c7f0-4019-a53a-645f769b7b57	\N	\N	\N	\N
921ba732-657c-45e0-bf62-64c56d6830d1	\N	\N	afa956da-0144-48b7-8a20-5ed57b6f03db	7b9cee01-bef5-4cbb-88b0-77ab212ae881	2018-01-26 07:19:54.12	admin	\N	\N
\.


--
-- TOC entry 3149 (class 0 OID 16438)
-- Dependencies: 194
-- Data for Name: stormlv; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormlv (primarykey, class_m0, view_m0, createtime, creator, edittime, editor) FROM stdin;
\.


--
-- TOC entry 3178 (class 0 OID 16749)
-- Dependencies: 223
-- Data for Name: stormnetlockdata; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormnetlockdata (lockkey, username, lockdate) FROM stdin;
{816e18c6-79a6-4548-8695-37d9bc9d6f8c}	admin	2018-01-26 07:31:28.909
{57a3968f-af04-4326-b72d-973bb7149e0f}	admin	2018-01-26 07:57:55.276
\.


--
-- TOC entry 3150 (class 0 OID 16444)
-- Dependencies: 195
-- Data for Name: stormp; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormp (primarykey, subject_m0, agent_m0, createtime, creator, edittime, editor) FROM stdin;
8a4f5b9b-12cd-4599-911e-497b6e6f3fc3	816e18c6-79a6-4548-8695-37d9bc9d6f8c	7b9cee01-bef5-4cbb-88b0-77ab212ae881	\N	\N	\N	\N
160e05bc-41e6-4e16-9135-16debc6515f6	816e18c6-79a6-4548-8695-37d9bc9d6f8c	590de25f-c7f0-4019-a53a-645f769b7b57	2018-01-26 07:57:35.711	admin	\N	\N
2057032d-f73b-4dcc-955b-bdbbb42174a9	f4347bc7-6e1a-447c-8f87-87d8f34329ee	590de25f-c7f0-4019-a53a-645f769b7b57	2018-01-26 07:57:35.712	admin	\N	\N
83699e83-6d99-44e1-9023-fde55e00a5a8	6fa6221e-c2f9-4a94-b560-b23575410bc2	590de25f-c7f0-4019-a53a-645f769b7b57	2018-01-26 07:57:35.712	admin	\N	\N
910bafbc-6d23-4d40-8a11-6e8906351c78	76c27031-bc9a-4103-a1ad-cd33987097a6	590de25f-c7f0-4019-a53a-645f769b7b57	2018-01-26 07:57:35.712	admin	\N	\N
4520011a-23ac-4aa3-9ff9-71869aa9c5a4	c50c9d56-1a81-4b88-9c11-84559caa7f41	590de25f-c7f0-4019-a53a-645f769b7b57	2018-01-26 07:57:35.712	admin	\N	\N
d94545ac-c0f8-4671-b291-c784feeb56e0	57a3968f-af04-4326-b72d-973bb7149e0f	590de25f-c7f0-4019-a53a-645f769b7b57	2018-01-26 07:57:35.713	admin	\N	\N
ecde3609-3bc3-4298-b50a-9dd61978b649	9a39df06-e28e-4d44-a791-29cb705a0127	590de25f-c7f0-4019-a53a-645f769b7b57	2018-01-26 07:57:35.713	admin	\N	\N
661f7ba3-3dea-4997-bb27-19e3553261d6	37b65397-850e-445a-ad1b-ee34fa92eda2	590de25f-c7f0-4019-a53a-645f769b7b57	2018-01-26 07:57:35.713	admin	\N	\N
fcbed4cb-d102-4a17-9f12-6b7259b37e59	35963c75-7a21-49b4-9222-0e4dcd356a67	590de25f-c7f0-4019-a53a-645f769b7b57	2018-01-26 07:57:35.713	admin	\N	\N
1d099001-49f4-4c40-a078-54a59f63f2c9	1d742d0b-e6fd-4b56-8363-61c5f2178c45	590de25f-c7f0-4019-a53a-645f769b7b57	2018-01-26 07:57:51.773	admin	\N	\N
44e81883-e2d4-4f68-9be6-dfa5eb937545	1d742d0b-e6fd-4b56-8363-61c5f2178c45	7b9cee01-bef5-4cbb-88b0-77ab212ae881	2018-01-26 07:57:51.773	admin	\N	\N
\.


--
-- TOC entry 3151 (class 0 OID 16450)
-- Dependencies: 196
-- Data for Name: storms; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY storms (primarykey, name, type, isattribute, isoperation, isview, isclass, sharedoper, createtime, creator, edittime, editor) FROM stdin;
816e18c6-79a6-4548-8695-37d9bc9d6f8c	ICSSoft.STORMNET.Reports.StormReportDocument	\N	f	f	f	t	t	\N	\N	\N	\N
1d742d0b-e6fd-4b56-8363-61c5f2178c45	ICSSoft.STORMNET.Reports.StormReport	\N	f	f	f	t	t	\N	\N	\N	\N
76c27031-bc9a-4103-a1ad-cd33987097a6	NewPlatform.Flexberry.HighwaySB.StatRecord	\N	f	f	f	t	t	2018-01-26 07:55:32.615	admin	\N	\N
35963c75-7a21-49b4-9222-0e4dcd356a67	NewPlatform.Flexberry.HighwaySB.ТипСообщения	\N	f	f	f	t	t	2018-01-26 07:55:48.479	admin	\N	\N
6fa6221e-c2f9-4a94-b560-b23575410bc2	NewPlatform.Flexberry.HighwaySB.OutboundMessageTypeRestriction	\N	f	f	f	t	t	2018-01-26 07:56:00.749	admin	\N	\N
57a3968f-af04-4326-b72d-973bb7149e0f	NewPlatform.Flexberry.HighwaySB.Клиент	\N	f	f	f	t	t	2018-01-26 07:56:12.456	admin	\N	\N
c50c9d56-1a81-4b88-9c11-84559caa7f41	NewPlatform.Flexberry.HighwaySB.StatSetting	\N	f	f	f	t	t	2018-01-26 07:56:24.101	admin	\N	\N
9a39df06-e28e-4d44-a791-29cb705a0127	NewPlatform.Flexberry.HighwaySB.Подписка	\N	f	f	f	t	t	2018-01-26 07:56:37.001	admin	\N	\N
37b65397-850e-445a-ad1b-ee34fa92eda2	NewPlatform.Flexberry.HighwaySB.Сообщение	\N	f	f	f	t	t	2018-01-26 07:56:48.265	admin	\N	\N
f4347bc7-6e1a-447c-8f87-87d8f34329ee	NewPlatform.Flexberry.HighwaySB.CompressionSetting	\N	f	f	f	t	t	2018-01-26 07:57:00.844	admin	\N	\N
\.


--
-- TOC entry 3179 (class 0 OID 16757)
-- Dependencies: 224
-- Data for Name: stormsettings; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormsettings (primarykey, module, name, value, "User") FROM stdin;
\.


--
-- TOC entry 3182 (class 0 OID 16781)
-- Dependencies: 227
-- Data for Name: stormwebsearch; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY stormwebsearch (primarykey, name, "Order", presentview, detailedview, filtersetting_m0) FROM stdin;
\.


--
-- TOC entry 3164 (class 0 OID 16655)
-- Dependencies: 209
-- Data for Name: substatisticsmonitor; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY substatisticsmonitor (primarykey, "Категория", "Код", "Наименование", createtime, creator, edittime, editor, "Подписка", statisticsmonitor) FROM stdin;
\.


--
-- TOC entry 3185 (class 0 OID 16805)
-- Dependencies: 230
-- Data for Name: usersetting; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY usersetting (primarykey, appname, username, userguid, modulename, moduleguid, settname, settguid, settlastaccesstime, strval, txtval, intval, boolval, guidval, decimalval, datetimeval) FROM stdin;
1ebe30bf-b9ea-4730-929b-d3816e9f86d4	\N	admin	\N	~/flexberry/SecurityUsersListctl00$ContentPlaceHolder1$ctl03$BottomPager	\N	ResultsPerPageProperty	\N	2018-01-26 07:19:30.553	\N	<SOAP-ENV:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:SOAP-ENC="http://schemas.xmlsoap.org/soap/encoding/" xmlns:SOAP-ENV="http://schemas.xmlsoap.org/soap/envelope/" xmlns:clr="http://schemas.microsoft.com/clr/" SOAP-ENV:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">\n  <SOAP-ENV:Body>\n    <xsd:int id="ref-1">\n      <m_value>10</m_value>\n    </xsd:int>\n  </SOAP-ENV:Body>\n</SOAP-ENV:Envelope>	\N	\N	\N	\N	\N
4791a1c2-ef14-4eb2-b55c-f01c3b457460	\N	admin	\N	~/flexberry/SecurityUsersListctl00$ContentPlaceHolder1$ctl03$TopPager	\N	ResultsPerPageProperty	\N	2018-01-26 07:19:30.637	\N	<SOAP-ENV:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:SOAP-ENC="http://schemas.xmlsoap.org/soap/encoding/" xmlns:SOAP-ENV="http://schemas.xmlsoap.org/soap/envelope/" xmlns:clr="http://schemas.microsoft.com/clr/" SOAP-ENV:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">\n  <SOAP-ENV:Body>\n    <xsd:int id="ref-1">\n      <m_value>10</m_value>\n    </xsd:int>\n  </SOAP-ENV:Body>\n</SOAP-ENV:Envelope>	\N	\N	\N	\N	\N
e10c39f8-81e8-4732-a29c-3e038119f8b9	\N	admin	\N	~/forms/Audit/AuditEntityByObjectsL.aspx/ctl00$ContentPlaceHolder1$WebObjectListView1/AuditEntityByObjectsL	\N	ResultsPerPageProperty	\N	2018-01-26 07:31:05.948	\N	<SOAP-ENV:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:SOAP-ENC="http://schemas.xmlsoap.org/soap/encoding/" xmlns:SOAP-ENV="http://schemas.xmlsoap.org/soap/envelope/" xmlns:clr="http://schemas.microsoft.com/clr/" SOAP-ENV:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">\n  <SOAP-ENV:Body>\n    <xsd:int id="ref-1">\n      <m_value>10</m_value>\n    </xsd:int>\n  </SOAP-ENV:Body>\n</SOAP-ENV:Envelope>	\N	\N	\N	\N	\N
05110ae0-e53e-466c-a500-fc3a230d9d13	\N	admin	\N	~/forms/Audit/AuditEntityByObjectsE.aspx/ctl00$ContentPlaceHolder1$ctrlAuditFields/AuditFieldByObjectsE	\N	ResultsPerPageProperty	\N	2018-01-26 07:31:18.496	\N	<SOAP-ENV:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:SOAP-ENC="http://schemas.xmlsoap.org/soap/encoding/" xmlns:SOAP-ENV="http://schemas.xmlsoap.org/soap/envelope/" xmlns:clr="http://schemas.microsoft.com/clr/" SOAP-ENV:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">\n  <SOAP-ENV:Body>\n    <xsd:int id="ref-1">\n      <m_value>10</m_value>\n    </xsd:int>\n  </SOAP-ENV:Body>\n</SOAP-ENV:Envelope>	\N	\N	\N	\N	\N
2af0513c-c735-461d-a8b3-78a9a0ae5335	\N	admin	\N	~/flexberry/SecurityRolesListctl00$ContentPlaceHolder1$ctl03$BottomPager	\N	ResultsPerPageProperty	\N	2018-01-26 07:57:25.636	\N	<SOAP-ENV:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:SOAP-ENC="http://schemas.xmlsoap.org/soap/encoding/" xmlns:SOAP-ENV="http://schemas.xmlsoap.org/soap/envelope/" xmlns:clr="http://schemas.microsoft.com/clr/" SOAP-ENV:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">\n  <SOAP-ENV:Body>\n    <xsd:int id="ref-1">\n      <m_value>10</m_value>\n    </xsd:int>\n  </SOAP-ENV:Body>\n</SOAP-ENV:Envelope>	\N	\N	\N	\N	\N
b8fa658b-96dc-436d-99aa-e0b992b9e07e	\N	admin	\N	~/flexberry/SecurityClassesListctl00$ContentPlaceHolder1$ctl03$BottomPager	\N	ResultsPerPageProperty	\N	2018-01-26 07:29:36.471	\N	<SOAP-ENV:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:SOAP-ENC="http://schemas.xmlsoap.org/soap/encoding/" xmlns:SOAP-ENV="http://schemas.xmlsoap.org/soap/envelope/" xmlns:clr="http://schemas.microsoft.com/clr/" SOAP-ENV:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">\n  <SOAP-ENV:Body>\n    <xsd:int id="ref-1">\n      <m_value>10</m_value>\n    </xsd:int>\n  </SOAP-ENV:Body>\n</SOAP-ENV:Envelope>	\N	\N	\N	\N	\N
7a9ab29e-ef4b-4135-ab9c-7dd2f354d5f8	\N	admin	\N	~/flexberry/SecurityClassesListctl00$ContentPlaceHolder1$ctl03$TopPager	\N	ResultsPerPageProperty	\N	2018-01-26 07:29:36.509	\N	<SOAP-ENV:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:SOAP-ENC="http://schemas.xmlsoap.org/soap/encoding/" xmlns:SOAP-ENV="http://schemas.xmlsoap.org/soap/envelope/" xmlns:clr="http://schemas.microsoft.com/clr/" SOAP-ENV:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">\n  <SOAP-ENV:Body>\n    <xsd:int id="ref-1">\n      <m_value>10</m_value>\n    </xsd:int>\n  </SOAP-ENV:Body>\n</SOAP-ENV:Envelope>	\N	\N	\N	\N	\N
6c9bf6f7-b38c-48be-a23a-2120dce64109	\N	admin	\N	~/flexberry/SecurityRolesListctl00$ContentPlaceHolder1$ctl03$TopPager	\N	ResultsPerPageProperty	\N	2018-01-26 07:57:25.674	\N	<SOAP-ENV:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:SOAP-ENC="http://schemas.xmlsoap.org/soap/encoding/" xmlns:SOAP-ENV="http://schemas.xmlsoap.org/soap/envelope/" xmlns:clr="http://schemas.microsoft.com/clr/" SOAP-ENV:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">\n  <SOAP-ENV:Body>\n    <xsd:int id="ref-1">\n      <m_value>10</m_value>\n    </xsd:int>\n  </SOAP-ENV:Body>\n</SOAP-ENV:Envelope>	\N	\N	\N	\N	\N
81afdede-6627-4b25-8620-f6c6aafe99a2	\N	admin	\N	Sec_SubjectL	\N	columnWidths	\N	2018-01-26 07:57:20.9	\N	<SOAP-ENV:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:SOAP-ENC="http://schemas.xmlsoap.org/soap/encoding/" xmlns:SOAP-ENV="http://schemas.xmlsoap.org/soap/envelope/" xmlns:clr="http://schemas.microsoft.com/clr/" SOAP-ENV:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">\n  <SOAP-ENV:Body>\n    <SOAP-ENC:Array id="ref-1" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da" SOAP-ENC:arrayType="a1:ColumnWidthInfo[10]">\n      <item href="#ref-2" />\n      <item href="#ref-3" />\n      <item href="#ref-4" />\n      <item href="#ref-5" />\n      <item href="#ref-6" />\n      <item href="#ref-7" />\n      <item href="#ref-8" />\n      <item href="#ref-9" />\n      <item href="#ref-10" />\n      <item href="#ref-11" />\n    </SOAP-ENC:Array>\n    <a1:ColumnWidthInfo id="ref-2" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-12">MiniToolbar</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>80</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-3" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-13">Name</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>457</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-4" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-14">IsAttribute</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>90</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-5" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-15">IsOperation</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>90</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-6" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-16">IsView</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>90</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-7" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-17">IsClass</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>89</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-8" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-18">CreateTime</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>89</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-9" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-19">Creator</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>149</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-10" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-20">EditTime</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>88</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-11" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-21">Editor</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>149</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n  </SOAP-ENV:Body>\n</SOAP-ENV:Envelope>	\N	\N	\N	\N	\N
fb76b0f4-bd8a-4282-9602-4c982a422f07	\N	admin	\N	AuditEntityByObjectsL	\N	columnWidths	\N	2018-01-26 07:58:27.713	\N	<SOAP-ENV:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:SOAP-ENC="http://schemas.xmlsoap.org/soap/encoding/" xmlns:SOAP-ENV="http://schemas.xmlsoap.org/soap/envelope/" xmlns:clr="http://schemas.microsoft.com/clr/" SOAP-ENV:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">\n  <SOAP-ENV:Body>\n    <SOAP-ENC:Array id="ref-1" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da" SOAP-ENC:arrayType="a1:ColumnWidthInfo[7]">\n      <item href="#ref-2" />\n      <item href="#ref-3" />\n      <item href="#ref-4" />\n      <item href="#ref-5" />\n      <item href="#ref-6" />\n      <item href="#ref-7" />\n      <item href="#ref-8" />\n    </SOAP-ENC:Array>\n    <a1:ColumnWidthInfo id="ref-2" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-9">MiniToolbar</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>80</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-3" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-10">EditTime</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>90</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-4" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-11">Editor</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>150</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-5" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-12">ObjectType.Name</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>511</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-6" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-13">ObjectPrimaryKey</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>88</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-7" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-14">CreateTime</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>88</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n    <a1:ColumnWidthInfo id="ref-8" xmlns:a1="http://schemas.microsoft.com/clr/nsassem/ICSSoft.STORMNET.Web.Tools.WOLVFeatures/ICSSoft.STORMNET.Web.Tools%2C%20Version%3D1.0.0.1%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Ddd3c9e296c34a5da">\n      <_x003C_PropertyInViewName_x003E_k__BackingField id="ref-15">Creator</_x003C_PropertyInViewName_x003E_k__BackingField>\n      <_x003C_Width_x003E_k__BackingField>148</_x003C_Width_x003E_k__BackingField>\n    </a1:ColumnWidthInfo>\n  </SOAP-ENV:Body>\n</SOAP-ENV:Envelope>	\N	\N	\N	\N	\N
\.


--
-- TOC entry 3162 (class 0 OID 16642)
-- Dependencies: 207
-- Data for Name: watcher; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY watcher (primarykey, category, name, comment, isactive, "interval", address, requesttemplate, responsetemplate, connectionstring, type, soapaction, systemid, timetorespond, usetriplecheck, createtime, creator, edittime, editor, timeoflastcheck) FROM stdin;
\.


--
-- TOC entry 3166 (class 0 OID 16671)
-- Dependencies: 211
-- Data for Name: watcherexceptionsset; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY watcherexceptionsset (primarykey, active, watcher_m0, exceptionsset_m0) FROM stdin;
\.


--
-- TOC entry 3163 (class 0 OID 16650)
-- Dependencies: 208
-- Data for Name: watchergroupitem; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY watchergroupitem (primarykey, nestedwatcher, watcher) FROM stdin;
\.


--
-- TOC entry 3155 (class 0 OID 16589)
-- Dependencies: 200
-- Data for Name: watcherinformer; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY watcherinformer (primarykey, isactive, createtime, creator, edittime, editor, watcher, informer) FROM stdin;
\.


--
-- TOC entry 3160 (class 0 OID 16629)
-- Dependencies: 205
-- Data for Name: watchexception; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY watchexception (primarykey, name, active, period, months, days, start, durationhr, exceptionsset_m0) FROM stdin;
\.


--
-- TOC entry 3174 (class 0 OID 16720)
-- Dependencies: 219
-- Data for Name: Клиент; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY "Клиент" (primarykey, "Наименование", "Ид", "Адрес", dnsidentity, createtime, creator, edittime, editor) FROM stdin;
\.


--
-- TOC entry 3165 (class 0 OID 16663)
-- Dependencies: 210
-- Data for Name: Монитор; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY "Монитор" (primarykey, "Наименование", "ДоступенДругимПользователям", "Логин", createtime, creator, edittime, editor) FROM stdin;
\.


--
-- TOC entry 3169 (class 0 OID 16686)
-- Dependencies: 214
-- Data for Name: Подписка; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY "Подписка" (primarykey, "ДатаПрекращения", iscallback, "НеудПопытки", "ПередаватьПо", "Описание", createtime, creator, edittime, editor, "ТипСообщения_m0", "Клиент_m0", "Клиент_m1") FROM stdin;
\.


--
-- TOC entry 3159 (class 0 OID 16621)
-- Dependencies: 204
-- Data for Name: ПодпискаМонитора; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY "ПодпискаМонитора" (primarykey, "Категория", "Код", "Наименование", createtime, creator, edittime, editor, "Подписка", "Монитор") FROM stdin;
\.


--
-- TOC entry 3177 (class 0 OID 16741)
-- Dependencies: 222
-- Data for Name: Сообщение; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY "Сообщение" (primarykey, "ВремяСледующейОтправки", "Тело", "ВремяФормирования", "Отправитель", "ВложениеДляБазы", "Приоритет", "ИмяГруппы", "Отправляется", failscount, createtime, creator, edittime, editor, "Тэги", logmessages, "ТипСообщения_m0", "Получатель_m0", "Получатель_m1") FROM stdin;
\.


--
-- TOC entry 3173 (class 0 OID 16712)
-- Dependencies: 218
-- Data for Name: ТипСообщения; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY "ТипСообщения" (primarykey, "Наименование", "Ид", "Комментарий", createtime, creator, edittime, editor) FROM stdin;
\.


--
-- TOC entry 3175 (class 0 OID 16728)
-- Dependencies: 220
-- Data for Name: Тэг; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY "Тэг" (primarykey, "Имя", "Значение", createtime, creator, edittime, editor, "Сообщение_m0") FROM stdin;
\.


--
-- TOC entry 3170 (class 0 OID 16694)
-- Dependencies: 215
-- Data for Name: Шина; Type: TABLE DATA; Schema: public; Owner: flexberryhwsbuser
--

COPY "Шина" (primarykey, "interopАдрес", createtime, creator, edittime, editor, "Наименование", "Ид", "Адрес", dnsidentity) FROM stdin;
\.


--
-- TOC entry 2960 (class 2606 OID 16820)
-- Name: applicationlog applicationlog_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY applicationlog
    ADD CONSTRAINT applicationlog_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2936 (class 2606 OID 16740)
-- Name: compressionsetting compressionsetting_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY compressionsetting
    ADD CONSTRAINT compressionsetting_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2877 (class 2606 OID 16604)
-- Name: event event_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY event
    ADD CONSTRAINT event_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2882 (class 2606 OID 16620)
-- Name: exceptionsset exceptionsset_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY exceptionsset
    ADD CONSTRAINT exceptionsset_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2891 (class 2606 OID 16641)
-- Name: informer informer_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY informer
    ADD CONSTRAINT informer_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2924 (class 2606 OID 16706)
-- Name: logmsg logmsg_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY logmsg
    ADD CONSTRAINT logmsg_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2911 (class 2606 OID 16680)
-- Name: outboundmessagetyperestriction outboundmessagetyperestriction_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY outboundmessagetyperestriction
    ADD CONSTRAINT outboundmessagetyperestriction_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2871 (class 2606 OID 16588)
-- Name: scheme scheme_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY scheme
    ADD CONSTRAINT scheme_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2869 (class 2606 OID 16580)
-- Name: schemeitem schemeitem_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY schemeitem
    ADD CONSTRAINT schemeitem_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2863 (class 2606 OID 16575)
-- Name: schemeitemlink schemeitemlink_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY schemeitemlink
    ADD CONSTRAINT schemeitemlink_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2835 (class 2606 OID 16457)
-- Name: session session_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY session
    ADD CONSTRAINT session_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2880 (class 2606 OID 16612)
-- Name: statisticsmonitor statisticsmonitor_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY statisticsmonitor
    ADD CONSTRAINT statisticsmonitor_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2914 (class 2606 OID 16685)
-- Name: statrecord statrecord_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY statrecord
    ADD CONSTRAINT statrecord_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2927 (class 2606 OID 16711)
-- Name: statsetting statsetting_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY statsetting
    ADD CONSTRAINT statsetting_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2837 (class 2606 OID 16459)
-- Name: stormac stormac_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormac
    ADD CONSTRAINT stormac_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2948 (class 2606 OID 16772)
-- Name: stormadvlimit stormadvlimit_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormadvlimit
    ADD CONSTRAINT stormadvlimit_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2839 (class 2606 OID 16461)
-- Name: stormag stormag_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormag
    ADD CONSTRAINT stormag_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2964 (class 2606 OID 16833)
-- Name: stormauentity stormauentity_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormauentity
    ADD CONSTRAINT stormauentity_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2966 (class 2606 OID 16841)
-- Name: stormaufield stormaufield_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormaufield
    ADD CONSTRAINT stormaufield_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2962 (class 2606 OID 16825)
-- Name: stormauobjtype stormauobjtype_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormauobjtype
    ADD CONSTRAINT stormauobjtype_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2841 (class 2606 OID 16463)
-- Name: stormf stormf_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormf
    ADD CONSTRAINT stormf_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2954 (class 2606 OID 16796)
-- Name: stormfilterdetail stormfilterdetail_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormfilterdetail
    ADD CONSTRAINT stormfilterdetail_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2956 (class 2606 OID 16804)
-- Name: stormfilterlookup stormfilterlookup_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormfilterlookup
    ADD CONSTRAINT stormfilterlookup_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2950 (class 2606 OID 16780)
-- Name: stormfiltersetting stormfiltersetting_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormfiltersetting
    ADD CONSTRAINT stormfiltersetting_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2844 (class 2606 OID 16465)
-- Name: stormi stormi_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormi
    ADD CONSTRAINT stormi_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2846 (class 2606 OID 16467)
-- Name: stormla stormla_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormla
    ADD CONSTRAINT stormla_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2848 (class 2606 OID 16469)
-- Name: stormlg stormlg_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormlg
    ADD CONSTRAINT stormlg_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2850 (class 2606 OID 16471)
-- Name: stormlo stormlo_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormlo
    ADD CONSTRAINT stormlo_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2852 (class 2606 OID 16473)
-- Name: stormlr stormlr_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormlr
    ADD CONSTRAINT stormlr_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2854 (class 2606 OID 16475)
-- Name: stormlv stormlv_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormlv
    ADD CONSTRAINT stormlv_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2944 (class 2606 OID 16756)
-- Name: stormnetlockdata stormnetlockdata_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormnetlockdata
    ADD CONSTRAINT stormnetlockdata_pkey PRIMARY KEY (lockkey);


--
-- TOC entry 2856 (class 2606 OID 16477)
-- Name: stormp stormp_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormp
    ADD CONSTRAINT stormp_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2858 (class 2606 OID 16479)
-- Name: storms storms_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY storms
    ADD CONSTRAINT storms_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2946 (class 2606 OID 16764)
-- Name: stormsettings stormsettings_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormsettings
    ADD CONSTRAINT stormsettings_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2952 (class 2606 OID 16788)
-- Name: stormwebsearch stormwebsearch_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormwebsearch
    ADD CONSTRAINT stormwebsearch_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2901 (class 2606 OID 16662)
-- Name: substatisticsmonitor substatisticsmonitor_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY substatisticsmonitor
    ADD CONSTRAINT substatisticsmonitor_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2958 (class 2606 OID 16812)
-- Name: usersetting usersetting_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY usersetting
    ADD CONSTRAINT usersetting_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2893 (class 2606 OID 16649)
-- Name: watcher watcher_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY watcher
    ADD CONSTRAINT watcher_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2907 (class 2606 OID 16675)
-- Name: watcherexceptionsset watcherexceptionsset_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY watcherexceptionsset
    ADD CONSTRAINT watcherexceptionsset_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2897 (class 2606 OID 16654)
-- Name: watchergroupitem watchergroupitem_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY watchergroupitem
    ADD CONSTRAINT watchergroupitem_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2875 (class 2606 OID 16596)
-- Name: watcherinformer watcherinformer_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY watcherinformer
    ADD CONSTRAINT watcherinformer_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2889 (class 2606 OID 16633)
-- Name: watchexception watchexception_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY watchexception
    ADD CONSTRAINT watchexception_pkey PRIMARY KEY (primarykey);


--
-- TOC entry 2931 (class 2606 OID 16727)
-- Name: Клиент Клиент_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "Клиент"
    ADD CONSTRAINT "Клиент_pkey" PRIMARY KEY (primarykey);


--
-- TOC entry 2903 (class 2606 OID 16670)
-- Name: Монитор Монитор_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "Монитор"
    ADD CONSTRAINT "Монитор_pkey" PRIMARY KEY (primarykey);


--
-- TOC entry 2919 (class 2606 OID 16693)
-- Name: Подписка Подписка_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "Подписка"
    ADD CONSTRAINT "Подписка_pkey" PRIMARY KEY (primarykey);


--
-- TOC entry 2886 (class 2606 OID 16628)
-- Name: ПодпискаМонитора ПодпискаМонитора_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "ПодпискаМонитора"
    ADD CONSTRAINT "ПодпискаМонитора_pkey" PRIMARY KEY (primarykey);


--
-- TOC entry 2942 (class 2606 OID 16748)
-- Name: Сообщение Сообщение_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "Сообщение"
    ADD CONSTRAINT "Сообщение_pkey" PRIMARY KEY (primarykey);


--
-- TOC entry 2929 (class 2606 OID 16719)
-- Name: ТипСообщения ТипСообщения_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "ТипСообщения"
    ADD CONSTRAINT "ТипСообщения_pkey" PRIMARY KEY (primarykey);


--
-- TOC entry 2934 (class 2606 OID 16735)
-- Name: Тэг Тэг_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "Тэг"
    ADD CONSTRAINT "Тэг_pkey" PRIMARY KEY (primarykey);


--
-- TOC entry 2921 (class 2606 OID 16701)
-- Name: Шина Шина_pkey; Type: CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "Шина"
    ADD CONSTRAINT "Шина_pkey" PRIMARY KEY (primarykey);


--
-- TOC entry 2925 (class 1259 OID 17003)
-- Name: index046c3866fbc8480091f4365569b17940; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index046c3866fbc8480091f4365569b17940 ON statsetting USING btree ("Подписка");


--
-- TOC entry 2864 (class 1259 OID 16865)
-- Name: index12532284d13c4395b5cd147523581056; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index12532284d13c4395b5cd147523581056 ON schemeitem USING btree (watcher);


--
-- TOC entry 2865 (class 1259 OID 16877)
-- Name: index17480c3c069446b39baeed78270e2193; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index17480c3c069446b39baeed78270e2193 ON schemeitem USING btree ("Клиент");


--
-- TOC entry 2922 (class 1259 OID 16997)
-- Name: index17aca880132d45c393bb08557e1a4964; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index17aca880132d45c393bb08557e1a4964 ON logmsg USING btree (msgid);


--
-- TOC entry 2938 (class 1259 OID 17021)
-- Name: index2270586f025f4bfda9ee78883080532f; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index2270586f025f4bfda9ee78883080532f ON "Сообщение" USING btree ("ТипСообщения_m0");


--
-- TOC entry 2904 (class 1259 OID 16955)
-- Name: index2ec2b0f23f5c4cb698c84fd9f302b8e7; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index2ec2b0f23f5c4cb698c84fd9f302b8e7 ON watcherexceptionsset USING btree (exceptionsset_m0);


--
-- TOC entry 2905 (class 1259 OID 16949)
-- Name: index30fb67a043d940a48bcf19f159a699e1; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index30fb67a043d940a48bcf19f159a699e1 ON watcherexceptionsset USING btree (watcher_m0);


--
-- TOC entry 2939 (class 1259 OID 17027)
-- Name: index33be66f96451420ba9c3ec5aeacee178; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index33be66f96451420ba9c3ec5aeacee178 ON "Сообщение" USING btree ("Получатель_m0");


--
-- TOC entry 2915 (class 1259 OID 16979)
-- Name: index3a335ce60e484dcc868a6ee5dca6ef84; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index3a335ce60e484dcc868a6ee5dca6ef84 ON "Подписка" USING btree ("ТипСообщения_m0");


--
-- TOC entry 2937 (class 1259 OID 17015)
-- Name: index3b7c4237de6f49d8945f50ba960e9fa0; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index3b7c4237de6f49d8945f50ba960e9fa0 ON compressionsetting USING btree (statsetting);


--
-- TOC entry 2883 (class 1259 OID 16913)
-- Name: index48a90e66bfcf44e4b6e9ef3f0553e7ca; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index48a90e66bfcf44e4b6e9ef3f0553e7ca ON "ПодпискаМонитора" USING btree ("Монитор");


--
-- TOC entry 2872 (class 1259 OID 16889)
-- Name: index4e3191359a1246c0accc247853330720; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index4e3191359a1246c0accc247853330720 ON watcherinformer USING btree (watcher);


--
-- TOC entry 2932 (class 1259 OID 17009)
-- Name: index5020d5dc38d64b5abd03fa5de5265688; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index5020d5dc38d64b5abd03fa5de5265688 ON "Тэг" USING btree ("Сообщение_m0");


--
-- TOC entry 2887 (class 1259 OID 16919)
-- Name: index6021a1c8b5694688a4908ac6a1590275; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index6021a1c8b5694688a4908ac6a1590275 ON watchexception USING btree (exceptionsset_m0);


--
-- TOC entry 2873 (class 1259 OID 16895)
-- Name: index62bd67bef9c74a759a5743533e013bd3; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index62bd67bef9c74a759a5743533e013bd3 ON watcherinformer USING btree (informer);


--
-- TOC entry 2859 (class 1259 OID 16859)
-- Name: index75f68507430548369e6059ed8e68160c; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index75f68507430548369e6059ed8e68160c ON schemeitemlink USING btree (scheme);


--
-- TOC entry 2898 (class 1259 OID 16937)
-- Name: index7bd0771f151842479fef8d2517cb6221; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index7bd0771f151842479fef8d2517cb6221 ON substatisticsmonitor USING btree ("Подписка");


--
-- TOC entry 2908 (class 1259 OID 16961)
-- Name: index7d51d08764ca443e894e10a61f1e6152; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index7d51d08764ca443e894e10a61f1e6152 ON outboundmessagetyperestriction USING btree ("ТипСообщения");


--
-- TOC entry 2866 (class 1259 OID 16871)
-- Name: index84f3ab0aed014880ada778c20540df47; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index84f3ab0aed014880ada778c20540df47 ON schemeitem USING btree (groupscheme);


--
-- TOC entry 2909 (class 1259 OID 16967)
-- Name: index877e066eca4e47e3b3271a39508f7010; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX index877e066eca4e47e3b3271a39508f7010 ON outboundmessagetyperestriction USING btree ("Клиент");


--
-- TOC entry 2867 (class 1259 OID 16883)
-- Name: indexa022c8e1fde24133b2c2dc5f0be14877; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX indexa022c8e1fde24133b2c2dc5f0be14877 ON schemeitem USING btree (scheme);


--
-- TOC entry 2940 (class 1259 OID 17033)
-- Name: indexa08030c065d1454f9fa1bf8558eee8e2; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX indexa08030c065d1454f9fa1bf8558eee8e2 ON "Сообщение" USING btree ("Получатель_m1");


--
-- TOC entry 2894 (class 1259 OID 16925)
-- Name: indexa5d3464f360f4d05ab469cf5dcafbcac; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX indexa5d3464f360f4d05ab469cf5dcafbcac ON watchergroupitem USING btree (nestedwatcher);


--
-- TOC entry 2860 (class 1259 OID 16853)
-- Name: indexb0297f72310644588918450460c025cd; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX indexb0297f72310644588918450460c025cd ON schemeitemlink USING btree (source);


--
-- TOC entry 2912 (class 1259 OID 16973)
-- Name: indexc889fe71204c4cffbf5f220c5f85fc8c; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX indexc889fe71204c4cffbf5f220c5f85fc8c ON statrecord USING btree (statsetting);


--
-- TOC entry 2916 (class 1259 OID 16985)
-- Name: indexcfdf2d9e7ade49a5a890ed2b3752012b; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX indexcfdf2d9e7ade49a5a890ed2b3752012b ON "Подписка" USING btree ("Клиент_m0");


--
-- TOC entry 2917 (class 1259 OID 16991)
-- Name: indexd52e99ed0d4f420a83b5280b611b1927; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX indexd52e99ed0d4f420a83b5280b611b1927 ON "Подписка" USING btree ("Клиент_m1");


--
-- TOC entry 2899 (class 1259 OID 16943)
-- Name: indexd551be30a697482c8088736f0b6ee652; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX indexd551be30a697482c8088736f0b6ee652 ON substatisticsmonitor USING btree (statisticsmonitor);


--
-- TOC entry 2861 (class 1259 OID 16847)
-- Name: indexdafc599bf30b4d56892e172856d8154a; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX indexdafc599bf30b4d56892e172856d8154a ON schemeitemlink USING btree (target);


--
-- TOC entry 2895 (class 1259 OID 16931)
-- Name: indexdd5df0fdc52f4836a72739352ac228cb; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX indexdd5df0fdc52f4836a72739352ac228cb ON watchergroupitem USING btree (watcher);


--
-- TOC entry 2878 (class 1259 OID 16901)
-- Name: indexe817a9d180fd49eaa1978c9141bc8598; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX indexe817a9d180fd49eaa1978c9141bc8598 ON event USING btree (watcher);


--
-- TOC entry 2884 (class 1259 OID 16907)
-- Name: indexf5ce3027173d47eb81d35a7997443b29; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX indexf5ce3027173d47eb81d35a7997443b29 ON "ПодпискаМонитора" USING btree ("Подписка");


--
-- TOC entry 2842 (class 1259 OID 16480)
-- Name: stormi_iuser_m0; Type: INDEX; Schema: public; Owner: flexberryhwsbuser
--

CREATE INDEX stormi_iuser_m0 ON stormi USING btree (user_m0);


--
-- TOC entry 3006 (class 2606 OID 16968)
-- Name: statrecord fk08d251c0d56241d79bd58c5541a15385; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY statrecord
    ADD CONSTRAINT fk08d251c0d56241d79bd58c5541a15385 FOREIGN KEY (statsetting) REFERENCES statsetting(primarykey);


--
-- TOC entry 2967 (class 2606 OID 16481)
-- Name: stormac fk0a28b4709af04dbe94544cd78959afad; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormac
    ADD CONSTRAINT fk0a28b4709af04dbe94544cd78959afad FOREIGN KEY (permition_m0) REFERENCES stormp(primarykey);


--
-- TOC entry 2970 (class 2606 OID 16486)
-- Name: stormi fk0db76705a258406283a4734906a37ae9; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormi
    ADD CONSTRAINT fk0db76705a258406283a4734906a37ae9 FOREIGN KEY (user_m0) REFERENCES stormag(primarykey);


--
-- TOC entry 3016 (class 2606 OID 17028)
-- Name: Сообщение fk14ad53fe5327408aa6c10dbfb74f998e; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "Сообщение"
    ADD CONSTRAINT fk14ad53fe5327408aa6c10dbfb74f998e FOREIGN KEY ("Получатель_m1") REFERENCES "Шина"(primarykey);


--
-- TOC entry 2991 (class 2606 OID 16878)
-- Name: schemeitem fk190cb1dd213946868f73a232dca84d1d; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY schemeitem
    ADD CONSTRAINT fk190cb1dd213946868f73a232dca84d1d FOREIGN KEY (scheme) REFERENCES scheme(primarykey);


--
-- TOC entry 3003 (class 2606 OID 16950)
-- Name: watcherexceptionsset fk1c2326be066c4c22a87e48c6746b2dff; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY watcherexceptionsset
    ADD CONSTRAINT fk1c2326be066c4c22a87e48c6746b2dff FOREIGN KEY (exceptionsset_m0) REFERENCES exceptionsset(primarykey);


--
-- TOC entry 2996 (class 2606 OID 16908)
-- Name: ПодпискаМонитора fk1c6c064609fa4d6d9b8605e9aaf5a66f; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "ПодпискаМонитора"
    ADD CONSTRAINT fk1c6c064609fa4d6d9b8605e9aaf5a66f FOREIGN KEY ("Монитор") REFERENCES "Монитор"(primarykey);


--
-- TOC entry 2969 (class 2606 OID 16491)
-- Name: stormf fk2310a262f53c42378f80ae70069d90eb; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormf
    ADD CONSTRAINT fk2310a262f53c42378f80ae70069d90eb FOREIGN KEY (subject_m0) REFERENCES storms(primarykey);


--
-- TOC entry 2973 (class 2606 OID 16496)
-- Name: stormla fk236977ee6f54464bbe87c39426fa4fc8; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormla
    ADD CONSTRAINT fk236977ee6f54464bbe87c39426fa4fc8 FOREIGN KEY (attribute_m0) REFERENCES storms(primarykey);


--
-- TOC entry 2983 (class 2606 OID 16501)
-- Name: stormp fk283ba27ddef741db819aba57d7f2c457; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormp
    ADD CONSTRAINT fk283ba27ddef741db819aba57d7f2c457 FOREIGN KEY (agent_m0) REFERENCES stormag(primarykey);


--
-- TOC entry 3013 (class 2606 OID 17010)
-- Name: compressionsetting fk299a053adb534dd4929f4f91709124f7; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY compressionsetting
    ADD CONSTRAINT fk299a053adb534dd4929f4f91709124f7 FOREIGN KEY (statsetting) REFERENCES statsetting(primarykey);


--
-- TOC entry 2989 (class 2606 OID 16866)
-- Name: schemeitem fk378a2ea28e704a0f87dfb51f69e04ec4; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY schemeitem
    ADD CONSTRAINT fk378a2ea28e704a0f87dfb51f69e04ec4 FOREIGN KEY (groupscheme) REFERENCES scheme(primarykey);


--
-- TOC entry 3018 (class 2606 OID 17039)
-- Name: stormfilterdetail fk386cca90d4764181a11d0ffece32dfcc; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormfilterdetail
    ADD CONSTRAINT fk386cca90d4764181a11d0ffece32dfcc FOREIGN KEY (filtersetting_m0) REFERENCES stormfiltersetting(primarykey);


--
-- TOC entry 2975 (class 2606 OID 16506)
-- Name: stormlg fk3b6c3f5121a64fd9b8633c1627d2d0ff; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormlg
    ADD CONSTRAINT fk3b6c3f5121a64fd9b8633c1627d2d0ff FOREIGN KEY (user_m0) REFERENCES stormag(primarykey);


--
-- TOC entry 2981 (class 2606 OID 16511)
-- Name: stormlv fk3ca280d54c0b4b0484f33af218c70b6b; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormlv
    ADD CONSTRAINT fk3ca280d54c0b4b0484f33af218c70b6b FOREIGN KEY (view_m0) REFERENCES storms(primarykey);


--
-- TOC entry 2987 (class 2606 OID 16854)
-- Name: schemeitemlink fk43cba51c111d432ea15646dd988c980e; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY schemeitemlink
    ADD CONSTRAINT fk43cba51c111d432ea15646dd988c980e FOREIGN KEY (scheme) REFERENCES scheme(primarykey);


--
-- TOC entry 2979 (class 2606 OID 16516)
-- Name: stormlr fk4df333abe6964dbb825bf3fcb62c29c9; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormlr
    ADD CONSTRAINT fk4df333abe6964dbb825bf3fcb62c29c9 FOREIGN KEY (agent_m0) REFERENCES stormag(primarykey);


--
-- TOC entry 2999 (class 2606 OID 16926)
-- Name: watchergroupitem fk58c0c0df6a0c458f892caeae97c076cd; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY watchergroupitem
    ADD CONSTRAINT fk58c0c0df6a0c458f892caeae97c076cd FOREIGN KEY (watcher) REFERENCES watcher(primarykey);


--
-- TOC entry 3000 (class 2606 OID 16932)
-- Name: substatisticsmonitor fk5ed93d64870d4cab87fb0f12aa98e440; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY substatisticsmonitor
    ADD CONSTRAINT fk5ed93d64870d4cab87fb0f12aa98e440 FOREIGN KEY ("Подписка") REFERENCES "Подписка"(primarykey);


--
-- TOC entry 3017 (class 2606 OID 17034)
-- Name: stormwebsearch fk62ce8be9a59a422bab409efffc9cf694; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormwebsearch
    ADD CONSTRAINT fk62ce8be9a59a422bab409efffc9cf694 FOREIGN KEY (filtersetting_m0) REFERENCES stormfiltersetting(primarykey);


--
-- TOC entry 3008 (class 2606 OID 16980)
-- Name: Подписка fk664fb34122e74825a2e38fa08f614824; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "Подписка"
    ADD CONSTRAINT fk664fb34122e74825a2e38fa08f614824 FOREIGN KEY ("Клиент_m0") REFERENCES "Клиент"(primarykey);


--
-- TOC entry 3004 (class 2606 OID 16956)
-- Name: outboundmessagetyperestriction fk681f70f42a5b4b4aa1ea00ba4644f4a3; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY outboundmessagetyperestriction
    ADD CONSTRAINT fk681f70f42a5b4b4aa1ea00ba4644f4a3 FOREIGN KEY ("ТипСообщения") REFERENCES "ТипСообщения"(primarykey);


--
-- TOC entry 2976 (class 2606 OID 16521)
-- Name: stormlg fk708bcc4e370641a989511969252cc6f6; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormlg
    ADD CONSTRAINT fk708bcc4e370641a989511969252cc6f6 FOREIGN KEY (group_m0) REFERENCES stormag(primarykey);


--
-- TOC entry 3001 (class 2606 OID 16938)
-- Name: substatisticsmonitor fk7555eef6ad0943b39d69fcebe4aee64a; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY substatisticsmonitor
    ADD CONSTRAINT fk7555eef6ad0943b39d69fcebe4aee64a FOREIGN KEY (statisticsmonitor) REFERENCES statisticsmonitor(primarykey);


--
-- TOC entry 2982 (class 2606 OID 16526)
-- Name: stormlv fk828ac255291f4b28b3a4cbaac8a82bfc; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormlv
    ADD CONSTRAINT fk828ac255291f4b28b3a4cbaac8a82bfc FOREIGN KEY (class_m0) REFERENCES storms(primarykey);


--
-- TOC entry 3014 (class 2606 OID 17016)
-- Name: Сообщение fk841943ad6c184fccbc23f6332f352b1e; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "Сообщение"
    ADD CONSTRAINT fk841943ad6c184fccbc23f6332f352b1e FOREIGN KEY ("ТипСообщения_m0") REFERENCES "ТипСообщения"(primarykey);


--
-- TOC entry 2990 (class 2606 OID 16872)
-- Name: schemeitem fk8ad5106c22b447cbbc2112fbfcfa9cda; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY schemeitem
    ADD CONSTRAINT fk8ad5106c22b447cbbc2112fbfcfa9cda FOREIGN KEY ("Клиент") REFERENCES "Клиент"(primarykey);


--
-- TOC entry 2998 (class 2606 OID 16920)
-- Name: watchergroupitem fk8d88dec0cd924168bd69c7c0836c0dee; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY watchergroupitem
    ADD CONSTRAINT fk8d88dec0cd924168bd69c7c0836c0dee FOREIGN KEY (nestedwatcher) REFERENCES watcher(primarykey);


--
-- TOC entry 3010 (class 2606 OID 16992)
-- Name: logmsg fk927daf657e7d4641b666f413b3bd43bb; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY logmsg
    ADD CONSTRAINT fk927daf657e7d4641b666f413b3bd43bb FOREIGN KEY (msgid) REFERENCES "Сообщение"(primarykey);


--
-- TOC entry 2988 (class 2606 OID 16860)
-- Name: schemeitem fk9d1ee86b13b74bcd887c6721be9af3af; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY schemeitem
    ADD CONSTRAINT fk9d1ee86b13b74bcd887c6721be9af3af FOREIGN KEY (watcher) REFERENCES watcher(primarykey);


--
-- TOC entry 2977 (class 2606 OID 16531)
-- Name: stormlo fka733450fe75b4be8b69903ecc7b995a8; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormlo
    ADD CONSTRAINT fka733450fe75b4be8b69903ecc7b995a8 FOREIGN KEY (operation_m0) REFERENCES storms(primarykey);


--
-- TOC entry 2974 (class 2606 OID 16536)
-- Name: stormla fka8431fe961bb4308843028e7b7ad6fed; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormla
    ADD CONSTRAINT fka8431fe961bb4308843028e7b7ad6fed FOREIGN KEY (view_m0) REFERENCES storms(primarykey);


--
-- TOC entry 3002 (class 2606 OID 16944)
-- Name: watcherexceptionsset fka8611e5c53b7434cba6f706d04e9e783; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY watcherexceptionsset
    ADD CONSTRAINT fka8611e5c53b7434cba6f706d04e9e783 FOREIGN KEY (watcher_m0) REFERENCES watcher(primarykey);


--
-- TOC entry 3005 (class 2606 OID 16962)
-- Name: outboundmessagetyperestriction fka927cd8ed2c64852b6739f840b63facc; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY outboundmessagetyperestriction
    ADD CONSTRAINT fka927cd8ed2c64852b6739f840b63facc FOREIGN KEY ("Клиент") REFERENCES "Клиент"(primarykey);


--
-- TOC entry 3012 (class 2606 OID 17004)
-- Name: Тэг fkb0f0651a4c224862b8b6c13e85468f0a; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "Тэг"
    ADD CONSTRAINT fkb0f0651a4c224862b8b6c13e85468f0a FOREIGN KEY ("Сообщение_m0") REFERENCES "Сообщение"(primarykey);


--
-- TOC entry 2978 (class 2606 OID 16541)
-- Name: stormlo fkb61d2f63d66d4c678c4db0f8c8f3a2c7; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormlo
    ADD CONSTRAINT fkb61d2f63d66d4c678c4db0f8c8f3a2c7 FOREIGN KEY (class_m0) REFERENCES storms(primarykey);


--
-- TOC entry 3020 (class 2606 OID 17049)
-- Name: stormauentity fkb78d32876031499fa2bc00fae34bd86a; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormauentity
    ADD CONSTRAINT fkb78d32876031499fa2bc00fae34bd86a FOREIGN KEY (objecttype_m0) REFERENCES stormauobjtype(primarykey);


--
-- TOC entry 3007 (class 2606 OID 16974)
-- Name: Подписка fkbd2823b15877441793dc9f5953642c1a; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "Подписка"
    ADD CONSTRAINT fkbd2823b15877441793dc9f5953642c1a FOREIGN KEY ("ТипСообщения_m0") REFERENCES "ТипСообщения"(primarykey);


--
-- TOC entry 2984 (class 2606 OID 16546)
-- Name: stormp fkc2d0e7efece949538d72c5768952b70d; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormp
    ADD CONSTRAINT fkc2d0e7efece949538d72c5768952b70d FOREIGN KEY (subject_m0) REFERENCES storms(primarykey);


--
-- TOC entry 2986 (class 2606 OID 16848)
-- Name: schemeitemlink fkccc9a24ef0a54b088059ecccbffdeddb; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY schemeitemlink
    ADD CONSTRAINT fkccc9a24ef0a54b088059ecccbffdeddb FOREIGN KEY (source) REFERENCES schemeitem(primarykey);


--
-- TOC entry 2980 (class 2606 OID 16551)
-- Name: stormlr fkce93f3ffead34657bef2d63c4524273a; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormlr
    ADD CONSTRAINT fkce93f3ffead34657bef2d63c4524273a FOREIGN KEY (role_m0) REFERENCES stormag(primarykey);


--
-- TOC entry 3022 (class 2606 OID 17059)
-- Name: stormaufield fkd4a355a8bd014e99a0ae41193679586a; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormaufield
    ADD CONSTRAINT fkd4a355a8bd014e99a0ae41193679586a FOREIGN KEY (auditentity_m0) REFERENCES stormauentity(primarykey);


--
-- TOC entry 2995 (class 2606 OID 16902)
-- Name: ПодпискаМонитора fkd51d0bff99c846969d4ee5bb93927029; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "ПодпискаМонитора"
    ADD CONSTRAINT fkd51d0bff99c846969d4ee5bb93927029 FOREIGN KEY ("Подписка") REFERENCES "Подписка"(primarykey);


--
-- TOC entry 2994 (class 2606 OID 16896)
-- Name: event fkd57e73889dc341a0938847fe25aeafea; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY event
    ADD CONSTRAINT fkd57e73889dc341a0938847fe25aeafea FOREIGN KEY (watcher) REFERENCES watcher(primarykey);


--
-- TOC entry 3021 (class 2606 OID 17054)
-- Name: stormaufield fkd6657a1655fe4fdca18335ee02d32fc3; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormaufield
    ADD CONSTRAINT fkd6657a1655fe4fdca18335ee02d32fc3 FOREIGN KEY (mainchange_m0) REFERENCES stormaufield(primarykey);


--
-- TOC entry 2968 (class 2606 OID 16556)
-- Name: stormac fkd9f68de7d20748b891c8d4c12a2d87cd; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormac
    ADD CONSTRAINT fkd9f68de7d20748b891c8d4c12a2d87cd FOREIGN KEY (filter_m0) REFERENCES stormf(primarykey);


--
-- TOC entry 2993 (class 2606 OID 16890)
-- Name: watcherinformer fke3b1607134794b7f95f89bd6fa50413b; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY watcherinformer
    ADD CONSTRAINT fke3b1607134794b7f95f89bd6fa50413b FOREIGN KEY (informer) REFERENCES informer(primarykey);


--
-- TOC entry 2985 (class 2606 OID 16842)
-- Name: schemeitemlink fke6401b03277f4d83947319821c6de158; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY schemeitemlink
    ADD CONSTRAINT fke6401b03277f4d83947319821c6de158 FOREIGN KEY (target) REFERENCES schemeitem(primarykey);


--
-- TOC entry 3009 (class 2606 OID 16986)
-- Name: Подписка fke6c055f8cd8444d7926f30de716a4693; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "Подписка"
    ADD CONSTRAINT fke6c055f8cd8444d7926f30de716a4693 FOREIGN KEY ("Клиент_m1") REFERENCES "Шина"(primarykey);


--
-- TOC entry 2971 (class 2606 OID 16561)
-- Name: stormi fke8774e3711cc4c1c97e863ac60f361ab; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormi
    ADD CONSTRAINT fke8774e3711cc4c1c97e863ac60f361ab FOREIGN KEY (agent_m0) REFERENCES stormag(primarykey);


--
-- TOC entry 3015 (class 2606 OID 17022)
-- Name: Сообщение fkf17180624a124c33b402e23fe35a4f6f; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY "Сообщение"
    ADD CONSTRAINT fkf17180624a124c33b402e23fe35a4f6f FOREIGN KEY ("Получатель_m0") REFERENCES "Клиент"(primarykey);


--
-- TOC entry 3011 (class 2606 OID 16998)
-- Name: statsetting fkf248c32aa34447fbabed63286249a468; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY statsetting
    ADD CONSTRAINT fkf248c32aa34447fbabed63286249a468 FOREIGN KEY ("Подписка") REFERENCES "Подписка"(primarykey);


--
-- TOC entry 2997 (class 2606 OID 16914)
-- Name: watchexception fkfa8349ebf7ee4201ab1a0ffac54c6233; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY watchexception
    ADD CONSTRAINT fkfa8349ebf7ee4201ab1a0ffac54c6233 FOREIGN KEY (exceptionsset_m0) REFERENCES exceptionsset(primarykey);


--
-- TOC entry 2992 (class 2606 OID 16884)
-- Name: watcherinformer fkfaa06871eb394a479d1cb925fc11d08e; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY watcherinformer
    ADD CONSTRAINT fkfaa06871eb394a479d1cb925fc11d08e FOREIGN KEY (watcher) REFERENCES watcher(primarykey);


--
-- TOC entry 3019 (class 2606 OID 17044)
-- Name: stormfilterlookup fkff8e5992cf7b4b6390b1b0561f3dfc89; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormfilterlookup
    ADD CONSTRAINT fkff8e5992cf7b4b6390b1b0561f3dfc89 FOREIGN KEY (filtersetting_m0) REFERENCES stormfiltersetting(primarykey);


--
-- TOC entry 2972 (class 2606 OID 16566)
-- Name: stormi stormi_fstormag_0; Type: FK CONSTRAINT; Schema: public; Owner: flexberryhwsbuser
--

ALTER TABLE ONLY stormi
    ADD CONSTRAINT stormi_fstormag_0 FOREIGN KEY (user_m0) REFERENCES stormag(primarykey);


-- Completed on 2018-01-26 13:01:50

--
-- PostgreSQL database dump complete
--

