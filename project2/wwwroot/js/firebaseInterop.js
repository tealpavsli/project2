import { initializeApp } from "https://www.gstatic.com/firebasejs/10.14.1/firebase-app.js";
import {
    getFirestore, doc, getDoc, setDoc
} from "https://www.gstatic.com/firebasejs/10.14.1/firebase-firestore.js";

const firebaseConfig = {
    apiKey: "AIzaSyAXCU-nxVoMKC-ydZRCSGrxHNdqBdeI1zM",
    authDomain: "heart-game-d00a9.firebaseapp.com",
    projectId: "heart-game-d00a9",
    storageBucket: "heart-game-d00a9.firebasestorage.app",
    messagingSenderId: "141049504648",
    appId: "1:141049504648:web:26fecfb1bb2f3f09be503d",
};

const app = initializeApp(firebaseConfig);
const db = getFirestore(app);

// Локальный запуск (dotnet run) и предпросмотр ветки test на GitHub Pages
// пишут в тестовую коллекцию, чтобы не смешивать с боевыми рекордами игроков
const isLocalTest = location.hostname === "localhost"
    || location.hostname === "127.0.0.1"
    || location.pathname.includes("test-preview");
const playersCollection = isLocalTest ? "players_test" : "players";

window.firebaseInterop = {
    // Возвращает рекорд игрока, или 0 если игрок новый
    getRecord: async function (playerName) {
        const ref = doc(db, playersCollection, playerName);
        const snap = await getDoc(ref);
        if (snap.exists()) {
            return snap.data().record ?? 0;
        }
        await setDoc(ref, { record: 0 });
        return 0;
    },

    // Сохраняет рекорд, если он больше текущего сохранённого
    saveRecord: async function (playerName, record) {
        const ref = doc(db, playersCollection, playerName);
        await setDoc(ref, { record: record }, { merge: true });
    }
};