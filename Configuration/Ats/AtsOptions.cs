namespace Configuration.Ats
{
    public class AtsOptions
    {
        public bool EnableParallelExecution { get; set; } = false;
        public List<string> ProtectedTokens { get; set; } = ["c#", ".net", "asp.net", "node.js", "f#", "c++"];
        
        /// <summary>
        /// Path to the Chrome User Data Directory. If null, defaults to LocalAppData\Google\Chrome\User Data.
        /// </summary>
        public string? ChromeUserDataDir { get; set; }
        
        // Stop words could be configurable or hardcoded list
        public List<string> StopWords { get; set; } = 
        [
            "a", "an", "the", "and", "or", "but", "if", "then", "else", "when", 
            "at", "by", "for", "from", "in", "into", "of", "off", "on", "onto", 
            "out", "over", "to", "up", "with", "is", "are", "was", "were", "be", 
            "been", "being", "have", "has", "had", "do", "does", "did", "say", 
            "says", "said", "go", "goes", "went", "gone", "get", "gets", "got", 
            "gotten", "make", "makes", "made", "know", "knows", "known", "think", 
            "thinks", "thought", "take", "takes", "took", "taken", "see", "sees", 
            "saw", "seen", "come", "comes", "came", "want", "wants", "wanted", 
            "look", "looks", "looked", "use", "uses", "used", "find", "finds", 
            "found", "give", "gives", "gave", "given", "tell", "tells", "told", 
            "work", "works", "worked", "call", "calls", "called", "try", "tries", 
            "tried", "ask", "asks", "asked", "need", "needs", "needed", "feel", 
            "feels", "felt", "become", "becomes", "became", "leave", "leaves", 
            "left", "put", "puts", "mean", "means", "meant", "keep", "keeps", 
            "kept", "let", "lets", "begin", "begins", "began", "begun", "seem", 
            "seems", "seemed", "help", "helps", "helped", "talk", "talks", 
            "talked", "turn", "turns", "turned", "start", "starts", "started", 
            "show", "shows", "showed", "shown", "hear", "hears", "heard", 
            "play", "plays", "played", "run", "runs", "ran", "move", "moves", 
            "moved", "like", "likes", "liked", "live", "lives", "lived", 
            "believe", "believes", "believed", "hold", "holds", "held", 
            "bring", "brings", "brought", "happen", "happens", "happened", 
            "must", "write", "writes", "wrote", "written", "provide", "provided", 
            "sit", "sits", "sat", "stand", "stands", "stood", "lose", "loses", 
            "lost", "pay", "pays", "paid", "meet", "meets", "met", "include", 
            "includes", "included", "continue", "continues", "continued", 
            "set", "sets", "learn", "learns", "learned", "change", "changes", 
            "changed", "lead", "leads", "led", "understand", "understands", 
            "understood", "watch", "watches", "watched", "follow", "follows", 
            "followed", "stop", "stops", "stopped", "create", "creates", 
            "created", "speak", "speaks", "spoke", "spoken", "read", "reads", 
            "allow", "allows", "allowed", "add", "adds", "added", "spend", 
            "spends", "spent", "grow", "grows", "grew", "grown", "open", 
            "opens", "opened", "walk", "walks", "walked", "win", "wins", 
            "won", "offer", "offers", "offered", "remember", "remembers", 
            "remembered", "love", "loves", "loved", "consider", "considers", 
            "considered", "appear", "appears", "appeared", "buy", "buys", 
            "bought", "wait", "waits", "waited", "serve", "serves", "served", 
            "die", "dies", "died", "send", "sends", "sent", "expect", 
            "expects", "expected", "build", "builds", "built", "stay", 
            "stays", "stayed", "fall", "falls", "fell", "fallen", "cut", 
            "cuts", "reach", "reaches", "reached", "kill", "kills", "killed", 
            "remain", "remains", "remained", "we", "us", "our", "you", "your", 
            "he", "him", "his", "she", "her", "they", "them", "their", "it", 
            "its", "who", "whom", "whose", "which", "what", "that", "this", 
            "these", "those", "can", "will", "would", "should", "could", "may", 
            "might", "must", "as"
        ];
    }
}
