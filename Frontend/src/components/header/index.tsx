import { FunctionalComponent, h } from "preact";
import { Link } from "preact-router/match";
import * as style from "./style.css";
import ky from "ky";
import { Meme, MemeIdentification } from "../../models";
import { route } from "preact-router";



const Header: FunctionalComponent = () => {
    loadMeme = () => {
        ky.get(`/api/memes/${this.props.threadId}/${this.props.memeId}`)
            .json<Meme>()
            .then(meme => this.setState({ meme }));
    };

    nextMeme = () => {
        return ky.get(`/api/memes`)
            .json<MemeIdentification>()
            .then(meme => route(`/${meme.threadId}/${meme.id}`));
    }


    return (
        <header class={style.header}>
            <h1>MEMESTER</h1>
            <nav>
                <Link activeClassName={style.active} href="/">
                    Home
                </Link>
                <Link activeClassName={style.active} href="/profile">
                    Me
                </Link>
                <Link activeClassName={style.active} href="/profile/john">
                    John
                </Link>
            </nav>
        </header>
    );
};

export default Header;
